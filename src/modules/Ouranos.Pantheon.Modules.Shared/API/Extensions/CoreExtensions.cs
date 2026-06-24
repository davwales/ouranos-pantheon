using JasperFx;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Features.Health;
using Ouranos.Pantheon.Modules.Shared.Features.Health.Checks;
using Ouranos.Pantheon.Modules.Shared.Infra.Flagsmith;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres;
using Ouranos.Pantheon.Modules.Shared.Infra.RabbitMq;
using Ouranos.Pantheon.Modules.Shared.WebSockets;
using Serilog;
using TickerQ.Dashboard.DependencyInjection;
using TickerQ.DependencyInjection;
using TickerQ.EntityFrameworkCore.DbContextFactory;
using TickerQ.EntityFrameworkCore.DependencyInjection;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.RabbitMQ;

namespace Ouranos.Pantheon.Modules.Shared.API.Extensions;

public static class CoreExtensions
{
    public static IHostApplicationBuilder AddOuranosCore(
        this IHostApplicationBuilder builder,
        IConfiguration configuration,
        IReadOnlyCollection<IPantheonModule> modules,
        Action<LoggerConfiguration>? logger = null
    )
    {
        Serilog.Debugging.SelfLog.Enable(msg =>
            Console.Error.WriteLine($"[Serilog SelfLog] {msg}")
        );

        var loggerConfig = new LoggerConfiguration().ReadFrom.Configuration(configuration);
        logger?.Invoke(loggerConfig);
        Log.Logger = loggerConfig.CreateLogger();

        builder.Services.ConfigureRest(configuration);
        builder.Services.Configure<QueryOptions>(
            configuration.GetSection(QueryOptions.SectionName)
        );
        builder.Services.AddCoreFlagsmithModule(configuration);

        var rabbit = configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>();

        builder.UseWolverine(opts =>
        {
            opts.UseRuntimeCompilation();

            // IOuranosMachineLearningClient is registered via AddHttpClient<TInterface,TImpl>(lambda),
            // which is an opaque factory Wolverine 6 cannot inline. Allowlist it for service location.
            opts.CodeGeneration.AlwaysUseServiceLocationFor<IOuranosMachineLearningClient>();

            opts.Discovery.IncludeAssembly(typeof(IPantheonModule).Assembly);

            opts.Discovery.CustomizeHandlerDiscovery(x =>
                x.Includes.Implements<IPantheonHandler>()
            );

            if (rabbit is { Host.Length: > 0 })
            {
                opts.Policies.DisableConventionalLocalRouting();

                opts.UseRabbitMq(factory =>
                    {
                        factory.HostName = rabbit.Host;
                        factory.UserName = rabbit.Username;
                        factory.Password = rabbit.Password;
                    })
                    .AutoProvision();

                if (rabbit.RetryCount.HasValue)
                {
                    var intervals = Enumerable
                        .Repeat(TimeSpan.FromMilliseconds(250), rabbit.RetryCount.Value)
                        .ToArray();

                    opts.Policies.OnException<InvalidOperationException>().MoveToErrorQueue();

                    opts.Policies.OnException<ArgumentException>().MoveToErrorQueue();

                    opts.Policies.OnException<Exception>().RetryWithCooldown(intervals);
                }
            }

            foreach (var module in modules)
            {
                opts.Discovery.IncludeAssembly(module.GetType().Assembly);
                module.ConfigureWolverine(opts, builder.Configuration);
            }
        });

        builder.Services.CritterStackDefaults(x =>
        {
            x.Development.ResourceAutoCreate = AutoCreate.All;
            x.Production.ResourceAutoCreate = AutoCreate.CreateOrUpdate;
        });

        foreach (var module in modules)
        {
            module.Build(builder);
        }

        var postgresOptions =
            configuration.GetSection(PostgresOptions.SectionName).Get<PostgresOptions>()
            ?? new PostgresOptions();

        builder.Services.AddTickerQ(options =>
        {
            options.AddOperationalStore(efOptions =>
            {
                efOptions.UseTickerQDbContext<TickerQDbContext>(db =>
                    db.UseNpgsql(
                            postgresOptions.GetConnectionString(),
                            o => o.MigrationsAssembly(typeof(CoreExtensions).Assembly.FullName)
                        )
                        .UseSnakeCaseNamingConvention()
                );
            });

            options.AddDashboard(dashboardOptions =>
                dashboardOptions.SetBasePath("/tickerq/dashboard")
            );
        });

        builder.Services.AddMemoryCache().AddSerilog();

        builder
            .Services.AddSingleton<WebSocketHealthState>()
            .Configure<HealthOptions>(configuration.GetSection(HealthOptions.SectionName))
            .AddSingleton<IHealthCheck, PostgresHealthCheck>()
            .AddSingleton<IHealthCheck, RabbitMqHealthCheck>()
            .AddSingleton<IHealthCheck, OuranosMlHealthCheck>()
            .AddSingleton<IHealthCheck, WebSocketHealthCheck>()
            .AddSingleton<IHealthCheck, TickerQHealthCheck>();

        return builder;
    }

    public static async Task<WebApplication> UseOuranosCore(
        this WebApplication app,
        IReadOnlyList<IPantheonModule> modules
    )
    {
        app.UseSerilogRequestLogging();

        var tickerQDbContextFactory = app.Services.GetRequiredService<
            IDbContextFactory<TickerQDbContext>
        >();
        await using var tickerQDbContext = await tickerQDbContextFactory.CreateDbContextAsync();
        await tickerQDbContext.Database.MigrateAsync();

        app.UseTickerQ();

        foreach (var module in modules)
        {
            module.MapEndpoints(app);
            await module.Configure(app);
        }

        return app;
    }
}

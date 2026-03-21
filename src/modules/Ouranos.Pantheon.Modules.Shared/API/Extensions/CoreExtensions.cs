using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres;
using Ouranos.Pantheon.Modules.Shared.Infra.RabbitMq;
using Serilog;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.RabbitMQ;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using TickerQ.Dashboard.DependencyInjection;
using TickerQ.DependencyInjection;
using TickerQ.EntityFrameworkCore.DbContextFactory;
using TickerQ.EntityFrameworkCore.DependencyInjection;

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
        var loggerConfig = new LoggerConfiguration().ReadFrom.Configuration(configuration);
        logger?.Invoke(loggerConfig);
        Log.Logger = loggerConfig.CreateLogger();

        builder.Services.ConfigureRest(configuration);
        builder.Services.Configure<QueryOptions>(configuration.GetSection(QueryOptions.SectionName));

        var rabbit = configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>();

        builder.UseWolverine(opts =>
            {
                opts.Discovery.IncludeAssembly(typeof(IPantheonModule).Assembly);

                opts.Discovery.CustomizeHandlerDiscovery(x =>
                    x.Includes.Implements<IPantheonHandler>()
                );

                if (rabbit is { Host.Length: > 0 })
                {
                    opts.Policies.DisableConventionalLocalRouting();

                    opts
                        .UseRabbitMq(factory =>
                            {
                                factory.HostName = rabbit.Host;
                                factory.UserName = rabbit.Username;
                                factory.Password = rabbit.Password;
                            }
                        )
                        .AutoProvision();

                    if (rabbit.RetryCount.HasValue)
                    {
                        var intervals = Enumerable
                            .Repeat(TimeSpan.FromMilliseconds(250), rabbit.RetryCount.Value)
                            .ToArray();

                        opts.Policies.OnException<Exception>().RetryWithCooldown(intervals);
                    }
                }

                foreach (var module in modules)
                {
                    opts.Discovery.IncludeAssembly(module.GetType().Assembly);
                    module.ConfigureWolverine(opts, builder.Configuration);
                }
            }
        );

        foreach (var module in modules)
        {
            module.Build(builder);
        }

        var postgresOptions = configuration.GetSection(PostgresOptions.SectionName).Get<PostgresOptions>()
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
                    }
                );

                options.AddDashboard(dashboardOptions => dashboardOptions.SetBasePath("/tickerq/dashboard"));
            }
        );

        builder.Services
            .AddMemoryCache()
            .AddSerilog();

        return builder;
    }

    public static async Task<WebApplication> UseOuranosCore(
        this WebApplication app,
        IReadOnlyList<IPantheonModule> modules
    )
    {
        app.UseSerilogRequestLogging();

        var tickerQDbContextFactory = app.Services.GetRequiredService<IDbContextFactory<TickerQDbContext>>();
        var tickerQDbContext = await tickerQDbContextFactory.CreateDbContextAsync();
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

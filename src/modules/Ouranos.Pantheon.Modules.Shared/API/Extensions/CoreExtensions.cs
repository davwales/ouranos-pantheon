using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Serilog;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

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

        builder.Services.AddMediator(m =>
            {
                foreach (var module in modules)
                {
                    module.ConfigureMediator(m);
                }
            }
        );

        foreach (var module in modules)
        {
            module.Build(builder);
        }

        builder.Services.AddSerilog();

        return builder;
    }

    public static async Task<WebApplication> UseOuranosCore(
        this WebApplication app,
        IReadOnlyList<IPantheonModule> modules
    )
    {
        app.UseSerilogRequestLogging();

        foreach (var module in modules)
        {
            module.MapEndpoints(app);
            await module.Configure(app);
        }

        return app;
    }
}

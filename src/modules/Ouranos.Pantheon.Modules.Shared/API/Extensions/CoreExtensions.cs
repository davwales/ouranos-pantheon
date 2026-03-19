using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application;
using Serilog;
using Wolverine;
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

        ((WebApplicationBuilder)builder).Host.UseWolverine(opts =>
        {
            opts.Discovery.IncludeAssembly(typeof(IPantheonModule).Assembly);
            foreach (var module in modules)
            {
                opts.Discovery.IncludeAssembly(module.GetType().Assembly);
            }

            opts.Discovery.CustomizeHandlerDiscovery(x =>
                x.Includes.Implements<IPantheonHandler>());
        });

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

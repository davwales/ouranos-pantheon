using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.Configuration;
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
        Action<IRequestExecutorBuilder>? gql = null,
        Action<LoggerConfiguration>? logger = null
    )
    {
        var loggerConfig = new LoggerConfiguration().ReadFrom.Configuration(configuration);
        logger?.Invoke(loggerConfig);
        Log.Logger = loggerConfig.CreateLogger();

        var gqlBuilder = builder.Services.ConfigureGraphQl(configuration, modules);
        gql?.Invoke(gqlBuilder);

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
        app.MapGraphQL();

        foreach (var module in modules)
        {
            await module.Configure(app);
        }

        return app;
    }
}
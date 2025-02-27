using HotChocolate.Execution.Configuration;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.API.Interfaces;
using Serilog;

namespace Ouranos.Pantheon.Core.API.Extensions;

public static class CoreExtensions
{
    public static IServiceCollection AddOuranosCore(
        this IServiceCollection services,
        IConfiguration configuration,
        IReadOnlyCollection<IOuranosModule> modules,
        Action<IRequestExecutorBuilder>? gql = null,
        Action<LoggerConfiguration>? logger = null
    )
    {
        var loggerConfig = new LoggerConfiguration().ReadFrom.Configuration(configuration);
        logger?.Invoke(loggerConfig);
        Log.Logger = loggerConfig.CreateLogger();

        var gqlBuilder = services.ConfigureGraphQl(configuration, modules);
        gql?.Invoke(gqlBuilder);

        services.AddMediator(m =>
        {
            foreach (var module in modules)
            {
                module.ConfigureMediator(m);
            }
        });

        foreach (var module in modules)
        {
            module.ConfigureServices(services, configuration);
        }

        return services.AddSerilog();
    }

    public static WebApplication UseOuranosCore(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        app.MapGraphQL();
        return app;
    }
}
using HotChocolate.Execution.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Talos.Olympus.Core.Infra.Mongo;

namespace Talos.Olympus.Core.API.Extensions;

public static class CoreExtensions
{
    public static IServiceCollection AddTalosCore(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IRequestExecutorBuilder>? gql = null,
        Action<LoggerConfiguration>? logger = null
    )
    {
        var loggerConfig = new LoggerConfiguration().ReadFrom.Configuration(configuration);
        logger?.Invoke(loggerConfig);
        Log.Logger = loggerConfig.CreateLogger();

        var gqlBuilder = services.ConfigureGraphQL(configuration);
        gql?.Invoke(gqlBuilder);

        return services
            .AddSerilog()
            .AddMongo(configuration)
            .AddDefaultMediatrHandlers();
    }

    public static WebApplication UseTalosCore(this WebApplication app)
    {
        app.MapGraphQL();
        app.UseSerilogRequestLogging();
        return app;
    }
}
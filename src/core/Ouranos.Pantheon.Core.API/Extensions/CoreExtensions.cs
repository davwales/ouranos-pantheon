using HotChocolate.Execution.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.Infra.Mongo;
using Ouranos.Pantheon.Core.Infra.OuranosMl;
using Serilog;

namespace Ouranos.Pantheon.Core.API.Extensions;

public static class CoreExtensions
{
    public static IServiceCollection AddOuranosCore(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IRequestExecutorBuilder>? gql = null,
        Action<LoggerConfiguration>? logger = null
    )
    {
        var loggerConfig = new LoggerConfiguration().ReadFrom.Configuration(configuration);
        logger?.Invoke(loggerConfig);
        Log.Logger = loggerConfig.CreateLogger();

        var gqlBuilder = services.ConfigureGraphQl(configuration);
        gql?.Invoke(gqlBuilder);

        return services
            .AddSerilog()
            .AddCoreMongo(configuration)
            .AddCoreOuranosMachineLearningModule(configuration)
            .AddDefaultMediatrHandlers();
    }

    public static WebApplication UseOuranosCore(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        app.MapGraphQL();
        return app;
    }
}
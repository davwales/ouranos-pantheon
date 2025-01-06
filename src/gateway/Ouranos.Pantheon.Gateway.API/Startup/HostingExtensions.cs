using HotChocolate.Fusion.Metadata;
using Serilog;

namespace Ouranos.Pantheon.Gateway.API.Startup;

public static class HostingExtensions
{
    private const string CorsPolicy = "AllowLocalAndServer";

    public static WebApplication ConfigureBuilder(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();

        builder.Services
            .AddHttpClient()
            .ConfigureCors(builder.Configuration)
            .RegisterGateway()
            .AddSerilog();

        return builder.Build();
    }

    public static WebApplication ConfigureApp(this WebApplication app)
    {
        app.UseCors(CorsPolicy);
        app.MapGraphQL();
        app.UseSerilogRequestLogging();
        return app;
    }

    private static IServiceCollection RegisterGateway(this IServiceCollection services)
    {
        return services
            .AddSingleton<IConfigurationRewriter, FusionConfigurationRewriter>()
            .AddFusionGatewayServer()
            .ConfigureFromFile("./Gateway.fgp")
            .CoreBuilder.ModifyOptions(o =>
            {
                o.EnableStream = true;
            })
            .Services;
    }

    private static IServiceCollection ConfigureCors(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var corsAllowedHosts = configuration.GetSection("CorsAllowedHosts").Get<string[]>() ?? [];
        return services.AddCors(options =>
            options.AddPolicy(CorsPolicy, builder =>
                builder
                    .WithOrigins(corsAllowedHosts)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
            )
        );
    }
}

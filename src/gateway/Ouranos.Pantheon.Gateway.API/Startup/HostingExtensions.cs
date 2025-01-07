using Ouranos.Pantheon.Core.API.Extensions;
using Ouranos.Pantheon.Service.Hermes.API;
using Ouranos.Pantheon.Service.Plutus.API;

namespace Ouranos.Pantheon.Gateway.API.Startup;

public static class HostingExtensions
{
    private const string CorsPolicy = "AllowLocalAndServer";
    
    public static WebApplication ConfigureBuilder(this WebApplicationBuilder builder)
    {
        builder.Services
            .ConfigureCors(builder.Configuration)
            .AddOuranosCore(builder.Configuration, gql => gql
                .ModifyOptions(o => { o.EnableStream = true; })
                .ModifyCostOptions(o => o.EnforceCostLimits = false) // TODO - Refactor queries for lower cost
                .AddHermesSchema()
                .AddPlutusSchema()
            )
            .AddHermesModule(builder.Configuration)
            .AddPlutusModule(builder.Configuration);

        return builder.Build();
    }

    public static WebApplication ConfigureApp(this WebApplication app)
    {
        app.UseCors(CorsPolicy);
        app.UseOuranosCore();
        return app;
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

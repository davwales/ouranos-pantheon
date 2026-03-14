using Ouranos.Pantheon.Core.API.Extensions;
using Ouranos.Pantheon.Core.API.Interfaces;
using Ouranos.Pantheon.Hermes.Service.API;
using Ouranos.Pantheon.Plutus.Service.API;

namespace Ouranos.Pantheon.Gateway.API.Startup;

public static class HostingExtensions
{
    private const string CorsPolicy = "AllowLocalAndServer";
    private static readonly IReadOnlyList<IOuranosModule> Modules = [new HermesModule(), new PlutusModule()];

    public static WebApplication ConfigureBuilder(this WebApplicationBuilder builder)
    {
        builder.Services
            .ConfigureCors(builder.Configuration)
            .AddOuranosCore(
                builder.Configuration,
                Modules,
                gql => gql
                    .ModifyOptions(o => { o.EnableStream = true; })
                    .ModifyCostOptions(o => o.EnforceCostLimits = false) // TODO - Refactor queries for lower cost
            );

        return builder.Build();
    }

    public static async Task<WebApplication> ConfigureApp(this WebApplication app)
    {
        app.UseCors(CorsPolicy);
        await app.UseOuranosCore(Modules);
        return app;
    }

    private static IServiceCollection ConfigureCors(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var corsAllowedHosts = configuration.GetSection("CorsAllowedHosts").Get<string[]>() ?? [];
        return services.AddCors(options =>
            options.AddPolicy(
                CorsPolicy,
                builder =>
                    builder
                        .WithOrigins(corsAllowedHosts)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
            )
        );
    }
}
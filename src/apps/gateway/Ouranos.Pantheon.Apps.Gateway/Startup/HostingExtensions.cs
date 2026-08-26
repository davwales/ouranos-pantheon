using Ouranos.Pantheon.Modules.Hermes;
using Ouranos.Pantheon.Modules.Hestia;
using Ouranos.Pantheon.Modules.Plutus;
using Ouranos.Pantheon.Modules.Shared;
using Ouranos.Pantheon.Modules.Shared.API.Extensions;
using Ouranos.Pantheon.Modules.Shared.Contract;

namespace Ouranos.Pantheon.Apps.Gateway.Startup;

public static class HostingExtensions
{
    private const string CorsPolicy = "AllowLocalAndServer";
    private static readonly IReadOnlyList<IPantheonModule> Modules =
    [
        new SharedModule(),
        new HermesModule(),
        new PlutusModule(),
        new HestiaModule(),
    ];

    public static WebApplication ConfigureBuilder(this WebApplicationBuilder builder)
    {
        builder
            .AddOuranosCore(builder.Configuration, Modules)
            .Services.ConfigureCors(builder.Configuration);

        return builder.Build();
    }

    public static async Task<WebApplication> ConfigureApp(this WebApplication app)
    {
        app.UseCors(CorsPolicy);
        await app.UseOuranosCore(Modules);
        return app;
    }

    private static void ConfigureCors(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var corsAllowedHosts = configuration.GetSection("CorsAllowedHosts").Get<string[]>() ?? [];

        services.AddCors(options =>
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

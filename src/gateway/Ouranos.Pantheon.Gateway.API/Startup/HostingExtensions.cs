using Ouranos.Pantheon.Core.API.Extensions;
using Ouranos.Pantheon.Service.Hermes.API;
using Ouranos.Pantheon.Service.Plutus.API;

namespace Ouranos.Pantheon.Gateway.API.Startup;

public static class HostingExtensions
{
    public static WebApplication ConfigureBuilder(this WebApplicationBuilder builder)
    {
        builder.Services
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

    public static WebApplication ConfigureApp(this WebApplication app) => app.UseOuranosCore();
}

using Ouranos.Pantheon.Core.API.Extensions;
using Ouranos.Pantheon.Service.Plutus.Application;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;
using Ouranos.Pantheon.Service.Plutus.Infra.Mongo;

namespace Ouranos.Pantheon.Service.Plutus.API.Startup;

public static class HostingExtensions
{
    public static WebApplication ConfigureBuilder(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddOuranosCore(builder.Configuration, gql => gql
                    .BindModelId<Market>()
                    .BindModelId<Symbol>()
                    .ModifyCostOptions(o => o.EnforceCostLimits = false) // TODO - Refactor queries for lower cost
            )
            .AddApplicationModule()
            .RegisterMongoBehaviors();

        return builder.Build();
    }

    public static WebApplication ConfigureApp(this WebApplication app)
    {
        return app.UseOuranosCore();
    }
}
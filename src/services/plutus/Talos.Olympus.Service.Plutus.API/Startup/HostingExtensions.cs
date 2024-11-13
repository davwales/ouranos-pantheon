using Talos.Olympus.Core.API.Extensions;
using Talos.Olympus.Service.Plutus.Application;
using Talos.Olympus.Service.Plutus.Domain.Markets;
using Talos.Olympus.Service.Plutus.Domain.Symbols;
using Talos.Olympus.Service.Plutus.Infra.Mongo;

namespace Talos.Olympus.Service.Plutus.API.Startup;

public static class HostingExtensions
{
    public static WebApplication ConfigureBuilder(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddTalosCore(builder.Configuration, gql => gql
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
        return app.UseTalosCore();
    }
}
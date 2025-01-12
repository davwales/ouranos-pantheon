using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Conventions;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Application.Interfaces.Subscriptions;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Application.Interfaces.Trades;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Infra.Universalis.Subscriptions;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Infra.Universalis.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Infra.Universalis;

public static class UniversalisModule
{
    public static IServiceCollection AddUniversalisModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<UniversalisOptions>(configuration.GetSection(UniversalisOptions.SectionName));

        var conventionPack = new ConventionPack
        {
            new CamelCaseElementNameConvention()
        };

        ConventionRegistry.Register(
            "CamelCaseElementNameConvention",
            conventionPack,
            t => true
        );

        return services
            .AddSingleton<ISetupSubscriptions, SetupSubscriptions>()
            .AddSingleton<IParseTrades, ParseTrades>();
    }
}
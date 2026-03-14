using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Plutus.DataLoader.Osrs.Application.Interfaces.Trades;
using Ouranos.Pantheon.Plutus.DataLoader.Osrs.Infra.OsrsWiki.Trades;

namespace Ouranos.Pantheon.Plutus.DataLoader.Osrs.Infra.OsrsWiki;

public static class OsrsWikiModule
{
    public static IServiceCollection AddOsrsWikiModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<OsrsWikiOptions>(configuration.GetSection(OsrsWikiOptions.SectionName));

        services
            .AddSingleton<IGetTrades, GetTrades>()
            .AddHttpClient<IWikiClient, WikiClient>((sp, client) =>
                {
                    var options = sp.GetRequiredService<IOptions<OsrsWikiOptions>>();
                    client.BaseAddress = new Uri(options.Value.BaseAddress);
                }
            );

        return services;
    }
}
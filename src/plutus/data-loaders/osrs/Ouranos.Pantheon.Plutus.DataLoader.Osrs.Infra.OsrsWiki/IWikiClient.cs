using Ouranos.Pantheon.Plutus.DataLoader.Osrs.Infra.OsrsWiki.Models;

namespace Ouranos.Pantheon.Plutus.DataLoader.Osrs.Infra.OsrsWiki;

public interface IWikiClient
{
    Task<List<Mapping>> GetMappings(CancellationToken cancellationToken = default);

    Task<PriceResponse> GetPrices(CancellationToken cancellationToken = default);
}
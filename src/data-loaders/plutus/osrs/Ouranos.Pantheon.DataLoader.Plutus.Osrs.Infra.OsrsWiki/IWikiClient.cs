using Ouranos.Pantheon.DataLoader.Plutus.Osrs.Application.Dtos;

namespace Ouranos.Pantheon.DataLoader.Plutus.Osrs.Infra.OsrsWiki;

public interface IWikiClient
{
    Task<List<Mapping>> GetMappings(CancellationToken cancellationToken = default);

    Task<PriceResponse> GetPrices(CancellationToken cancellationToken = default);
}
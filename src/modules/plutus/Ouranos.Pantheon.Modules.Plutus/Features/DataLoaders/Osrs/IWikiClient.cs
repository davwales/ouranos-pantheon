using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Osrs.Models;

namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Osrs;

public interface IWikiClient
{
    Task<List<Mapping>> GetMappings(CancellationToken cancellationToken = default);

    Task<PriceResponse> GetPrices(CancellationToken cancellationToken = default);
}

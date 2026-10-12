using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;

public interface IFlipDataQueryService
{
    /// <summary>
    ///     Loads side-aware 5-minute bars from <paramref name="from" /> (rounded down to the hour)
    ///     to <paramref name="to" /> for the market's liquid, buy-limited symbols.
    /// </summary>
    Task<FlipMarketData> LoadAsync(
        Id<Market> marketId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken
    );
}

using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Swing;

public interface ISwingDataQueryService
{
    Task<SwingMarketData> LoadDataAsync(
        Id<Market> marketId,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        int lookbackDays,
        CancellationToken cancellationToken
    );
}

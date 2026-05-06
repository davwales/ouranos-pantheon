using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;

public interface IBacktestDataQueryService
{
    Task<BacktestData> LoadDataAsync(
        Id<Market> marketId,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        CancellationToken cancellationToken,
        int lookbackDays = 0
    );
}

using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

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

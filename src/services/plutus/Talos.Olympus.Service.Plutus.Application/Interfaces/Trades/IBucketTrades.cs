using Talos.Olympus.Service.Plutus.Application.Models.Trades;
using Talos.Olympus.Service.Plutus.Domain.Trades;

namespace Talos.Olympus.Service.Plutus.Application.Interfaces.Trades;

public interface IBucketTrades
{
    IQueryable<BucketDto> GetBucketedTradesQuery(
        IQueryable<Trade> query,
        int numBuckets,
        CancellationToken cancellationToken = default
    );
}
using Ouranos.Pantheon.Plutus.Service.Application.Models.Trades;
using Ouranos.Pantheon.Plutus.Service.Domain.Trades;

namespace Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Trades;

public interface IBucketTrades
{
    IQueryable<BucketDto> GetBucketedTradesQuery(
        IQueryable<Trade> query,
        int numBuckets,
        CancellationToken cancellationToken = default
    );
}
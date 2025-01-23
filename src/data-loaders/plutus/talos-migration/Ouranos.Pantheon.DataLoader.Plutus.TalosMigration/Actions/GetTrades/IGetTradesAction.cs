using MongoDB.Driver;
using Ouranos.Pantheon.DataLoader.Plutus.TalosMigration.Models;

namespace Ouranos.Pantheon.DataLoader.Plutus.TalosMigration.Actions.GetTrades;

public interface IGetTradesAction
{
    Task<IAsyncCursor<TalosTrade>> GetTradesAsync(CancellationToken cancellationToken = default);
}
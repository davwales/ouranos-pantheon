using MongoDB.Driver;
using Ouranos.Pantheon.DataLoader.Plutus.TalosMigration.Models;

namespace Ouranos.Pantheon.DataLoader.Plutus.TalosMigration.Handlers.GetTrades;

public sealed record GetTradesResponse(IAsyncCursor<TalosTrade> Cursor);
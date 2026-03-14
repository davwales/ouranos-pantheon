using Ouranos.Pantheon.Plutus.DataLoader.Osrs.Application.Queries.Trades.GetTrades;

namespace Ouranos.Pantheon.Plutus.DataLoader.Osrs.Application.Interfaces.Trades;

public interface IGetTrades
{
    Task<List<GetTradesResponse>> GetTradesAsync(CancellationToken cancellationToken = default);
}
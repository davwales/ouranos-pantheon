using Ouranos.Pantheon.DataLoader.Plutus.Osrs.Application.Queries.Trades.GetTrades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Osrs.Application.Interfaces.Trades;

public interface IGetTrades
{
    Task<List<GetTradesResponse>> GetTradesAsync(CancellationToken cancellationToken = default);
}
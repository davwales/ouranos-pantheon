using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Application.Dtos;

namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Application.Interfaces.Trades;

public interface IParseTrades
{
    Task<List<TradeDto>> ParseTradeMessage(
        byte[] message,
        CancellationToken cancellationToken = default
    );
}
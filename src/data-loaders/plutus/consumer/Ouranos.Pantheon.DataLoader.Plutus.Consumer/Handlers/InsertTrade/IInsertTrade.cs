using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Consumer.Handlers.InsertTrade;

public interface IInsertTrade
{
    Task<Trade> InsertTradeAsync(
        InsertTradeInput input,
        CancellationToken cancellationToken = default
    );
}
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;

namespace Ouranos.Pantheon.Plutus.DataLoader.Consumer.Handlers.InsertTrade;

public interface IInsertTrade
{
    Task<Trade> InsertTradeAsync(
        InsertTradeInput input,
        CancellationToken cancellationToken = default
    );
}
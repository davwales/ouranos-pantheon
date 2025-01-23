using Ouranos.Pantheon.DataLoader.Plutus.Domain.Trades;
using Ouranos.Pantheon.DataLoader.Plutus.TalosMigration.Models;

namespace Ouranos.Pantheon.DataLoader.Plutus.TalosMigration.Actions.ConvertTrade;

public interface IConvertTradeAction
{
    TradeMessage? ConvertTrade(TalosTrade? trade);
}
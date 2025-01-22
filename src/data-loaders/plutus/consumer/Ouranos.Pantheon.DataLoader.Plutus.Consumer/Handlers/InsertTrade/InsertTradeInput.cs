using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Consumer.Handlers.InsertTrade;

public sealed record InsertTradeInput(
    Id<Market> MarketId,
    Id<Symbol> SymbolId,
    string SymbolName,
    string SymbolCode,
    string? SymbolSubcode,
    decimal Price,
    decimal Volume,
    DateTimeOffset Timestamp,
    AdditionalFields AdditionalFields,
    Guid? MessageId
) : ICommand<Trade>;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;

namespace Ouranos.Pantheon.Plutus.DataLoader.Consumer.Handlers.InsertTrade;

public sealed record InsertTradeInput(
    Symbol Symbol,
    decimal Price,
    decimal Volume,
    DateTimeOffset Timestamp,
    Guid? MessageId
) : ICommand<Trade>;
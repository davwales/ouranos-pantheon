using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Consumer.Handlers.InsertTrade;

public sealed record InsertTradeInput(
    Symbol Symbol,
    decimal Price,
    decimal Volume,
    DateTimeOffset Timestamp,
    Guid? MessageId
) : ICommand<Trade>;
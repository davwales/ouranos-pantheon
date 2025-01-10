using MediatR;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Consumer.Application.Commands.Trades.InsertTrade;

public sealed record InsertTradeInput(
    Id<Market> MarketId,
    Id<Symbol> SymbolId,
    string SymbolName,
    string SymbolCode,
    string? SymbolSubCode,
    long? Limit,
    decimal Price,
    decimal Volume,
    DateTimeOffset Timestamp
) : IRequest<Trade>;
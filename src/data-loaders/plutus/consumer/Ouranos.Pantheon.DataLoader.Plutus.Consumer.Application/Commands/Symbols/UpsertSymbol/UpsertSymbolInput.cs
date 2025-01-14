using MediatR;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Consumer.Application.Commands.Symbols.UpsertSymbol;

public sealed record UpsertSymbolInput(
    Id<Market> MarketId,
    string SymbolCode,
    string? SymbolSubcode,
    string SymbolName,
    AdditionalFields AdditionalFields
) : IRequest<Symbol>;
using MediatR;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

namespace Ouranos.Pantheon.DataLoader.Plutus.Consumer.Application.Commands.Symbols.UpsertSymbol;

public sealed record UpsertSymbolInput(
    Id<Market> MarketId,
    string SymbolCode,
    string? SymbolSubCode,
    string SymbolName,
    Dictionary<string, object?> AdditionalFields
) : IRequest<Symbol>;
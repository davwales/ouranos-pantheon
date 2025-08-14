using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;
using Ouranos.Pantheon.Plutus.Service.Domain.Trades;

namespace Ouranos.Pantheon.Plutus.DataLoader.Consumer.Handlers.UpsertSymbol;

public sealed record UpsertSymbolInput(
    Id<Market> MarketId,
    string SymbolCode,
    string? SymbolSubcode,
    string SymbolName,
    AdditionalFields AdditionalFields
) : ICommand<Symbol>;
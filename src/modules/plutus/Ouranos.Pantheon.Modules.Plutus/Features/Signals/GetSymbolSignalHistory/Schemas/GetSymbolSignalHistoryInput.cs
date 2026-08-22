using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignalHistory.Schemas;

public sealed record GetSymbolSignalHistoryInput(
    Id<Symbol> SymbolId,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    string? Types = null,
    InvestmentIntent? Intent = null
);

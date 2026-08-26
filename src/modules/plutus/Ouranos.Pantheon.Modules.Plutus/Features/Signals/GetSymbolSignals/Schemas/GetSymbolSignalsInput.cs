using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;

public sealed record GetSymbolSignalsInput(Id<Symbol> SymbolId, InvestmentIntent? Intent = null);

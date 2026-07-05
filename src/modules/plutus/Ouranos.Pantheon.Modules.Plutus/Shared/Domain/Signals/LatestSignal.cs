using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;

public sealed record LatestSignal(Id<Symbol> SymbolId, SignalType SignalType, decimal LastValue);

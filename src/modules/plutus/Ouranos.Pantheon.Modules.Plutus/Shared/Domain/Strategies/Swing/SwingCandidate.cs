using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Swing;

public sealed record SwingCandidate(Symbol Symbol, decimal Score, decimal Price);

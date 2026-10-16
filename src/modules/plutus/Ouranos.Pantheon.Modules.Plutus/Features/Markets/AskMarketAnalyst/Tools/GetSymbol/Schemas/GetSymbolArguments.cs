using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Tools.GetSymbol.Schemas;

public sealed record GetSymbolArguments(Id<Symbol> SymbolId);

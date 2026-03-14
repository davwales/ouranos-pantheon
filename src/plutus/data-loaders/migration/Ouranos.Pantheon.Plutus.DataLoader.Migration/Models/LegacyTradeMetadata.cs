using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;

namespace Ouranos.Pantheon.Plutus.DataLoader.Migration.Models;

public sealed record LegacyTradeMetadata(
    Id<Symbol> SymbolId
);
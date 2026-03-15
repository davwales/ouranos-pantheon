using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Plutus.DataLoader.Consumer.Handlers.UpsertSymbol;

public interface IUpsertSymbol
{
    Task<Symbol> UpsertSymbolAsync(
        UpsertSymbolInput input,
        CancellationToken cancellationToken = default
    );
}
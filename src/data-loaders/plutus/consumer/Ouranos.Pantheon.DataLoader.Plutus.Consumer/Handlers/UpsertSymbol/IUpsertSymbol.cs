using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

namespace Ouranos.Pantheon.DataLoader.Plutus.Consumer.Handlers.UpsertSymbol;

public interface IUpsertSymbol
{
    Task<Symbol> UpsertSymbolAsync(
        UpsertSymbolInput input,
        CancellationToken cancellationToken = default
    );
}
using HotChocolate;
using HotChocolate.Types;
using Ouranos.Pantheon.Core.API.Queries;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol;

[ExtendObjectType<Query>]
public sealed class GetSymbolQuery
{
    /// <summary>
    ///     Gets a symbol.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="symbolId">The query to be executed.</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>The symbol matching the given query.</returns>
    public async Task<Symbol> GetSymbol(
        [Service] IScopedDispatcher dispatcher,
        Id<Symbol> symbolId,
        CancellationToken cancellationToken = default
    )
    {
        return await dispatcher.Send(new GetSymbolInput(symbolId), cancellationToken);
    }
}

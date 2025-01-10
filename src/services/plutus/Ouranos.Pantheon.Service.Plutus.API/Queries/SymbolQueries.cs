using MediatR;
using Ouranos.Pantheon.Core.API.Queries;
using Ouranos.Pantheon.Core.Application.Queries.Common.GetAllEntities;
using Ouranos.Pantheon.Core.Application.Queries.Common.GetEntity;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

namespace Ouranos.Pantheon.Service.Plutus.API.Queries;

[ExtendObjectType<Query>]
public sealed class SymbolQueries
{
    /// <summary>
    ///     Gets all symbols.
    /// </summary>
    /// <param name="mediator">
    ///     <see cref="IMediator" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>Queryable list of all symbols.</returns>
    [UsePaging(IncludeTotalCount = true)]
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<Symbol>> GetAllSymbols(
        [Service] IMediator mediator,
        CancellationToken cancellationToken = default
    )
    {
        return await mediator.Send(new GetAllEntitiesInput<Symbol>(), cancellationToken);
    }

    /// <summary>
    ///     Gets a symbol by its identifier.
    /// </summary>
    /// <param name="mediator">
    ///     <see cref="IMediator" />
    /// </param>
    /// <param name="symbolId">Id of the symbol to get.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken" />.</param>
    /// <returns>The symbol matching the given query.</returns>
    public async Task<Symbol> GetSymbol(
        [Service] IMediator mediator,
        Id<Symbol> symbolId,
        CancellationToken cancellationToken = default
    )
    {
        return await mediator.Send(new GetEntityInput<Symbol>(symbolId), cancellationToken);
    }
}
using Ouranos.Pantheon.Core.API.Queries;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Application.Queries.Common.GetAllEntities;
using Ouranos.Pantheon.Core.Application.Queries.Common.GetEntity;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.SymbolGroups;

namespace Ouranos.Pantheon.Service.Plutus.API.Queries;

[ExtendObjectType<Query>]
public sealed class SymbolGroupQueries
{
    /// <summary>
    ///     Gets all symbol groups.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="cancellationToken"><see cref="CancellationToken" />.</param>
    /// <returns>List of all symbol groups.</returns>
    [UsePaging(IncludeTotalCount = true)]
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<SymbolGroup>> GetAllSymbolGroups(
        [Service] IScopedDispatcher dispatcher,
        CancellationToken cancellationToken = default
    )
    {
        var wrapper = await dispatcher.Send(new GetAllEntitiesInput<SymbolGroup>(), cancellationToken);
        return wrapper.Value;
    }

    /// <summary>
    ///     Retrieves a symbol group by its identifier.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="symbolGroupId">Id of the symbol group to get.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken" />.</param>
    /// <returns>The symbol group matching the given query.</returns>
    public async Task<SymbolGroup> GetSymbolGroup(
        [Service] IScopedDispatcher dispatcher,
        Id<SymbolGroup> symbolGroupId,
        CancellationToken cancellationToken = default
    )
    {
        return await dispatcher.Send(new GetEntityInput<SymbolGroup>(symbolGroupId), cancellationToken);
    }
}
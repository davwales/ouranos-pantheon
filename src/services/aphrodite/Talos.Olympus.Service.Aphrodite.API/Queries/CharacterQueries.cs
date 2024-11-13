using MediatR;
using Talos.Olympus.Core.API.Queries;
using Talos.Olympus.Core.Application.Queries.Common.GetAllEntities;
using Talos.Olympus.Core.Application.Queries.Common.GetEntity;
using Talos.Olympus.Core.Domain.Common;
using Talos.Olympus.Service.Aphrodite.Domain.Characters;

namespace Talos.Olympus.Service.Aphrodite.API.Queries;

[ExtendObjectType<Query>]
public sealed class CharacterQueries
{
    /// <summary>
    ///     Gets a character.
    /// </summary>
    /// <param name="mediator">
    ///     <see cref="IMediator" />
    /// </param>
    /// <param name="characterId">The query to be executed.</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>The character matching the given query.</returns>
    public async Task<Character> GetCharacter(
        [Service] IMediator mediator,
        Id<Character> characterId,
        CancellationToken cancellationToken = default
    )
    {
        return await mediator.Send(new GetEntityInput<Character>(characterId), cancellationToken);
    }

    /// <summary>
    ///     Gets a queryable list of characters.
    /// </summary>
    /// <param name="mediator"><see cref="IMediator" />.</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>A list of characters.</returns>
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<Character>> GetAllCharacters(
        [Service] IMediator mediator,
        CancellationToken cancellationToken = default
    )
    {
        return await mediator.Send(new GetAllEntitiesInput<Character>(), cancellationToken);
    }
}
using Ouranos.Pantheon.Core.API.Queries;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Application.Queries.Common.GetAllEntities;
using Ouranos.Pantheon.Core.Application.Queries.Common.GetEntity;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Hermes.Domain.Characters;

namespace Ouranos.Pantheon.Service.Hermes.API.Queries;

[ExtendObjectType<Query>]
public sealed class CharacterQueries
{
    /// <summary>
    ///     Gets a character.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="characterId">The query to be executed.</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>The character matching the given query.</returns>
    public async Task<Character> GetCharacter(
        [Service] IDispatcher dispatcher,
        Id<Character> characterId,
        CancellationToken cancellationToken = default
    )
    {
        return await dispatcher.Send(new GetEntityInput<Character>(characterId), cancellationToken);
    }

    /// <summary>
    ///     Gets a queryable list of characters.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />.
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>A list of characters.</returns>
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<Character>> GetAllCharacters(
        [Service] IDispatcher dispatcher,
        CancellationToken cancellationToken = default
    )
    {
        var wrapper = await dispatcher.Send(new GetAllEntitiesInput<Character>(), cancellationToken);
        return wrapper.Value;
    }
}
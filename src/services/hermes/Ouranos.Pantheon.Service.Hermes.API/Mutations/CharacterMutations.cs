using Ouranos.Pantheon.Core.API.Mutations;
using Ouranos.Pantheon.Core.Application.Commands.Common.DeleteEntity;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Hermes.Application.Commands.Characters.CreateCharacter;
using Ouranos.Pantheon.Service.Hermes.Application.Commands.Characters.UpdateCharacter;
using Ouranos.Pantheon.Service.Hermes.Domain.Characters;

namespace Ouranos.Pantheon.Service.Hermes.API.Mutations;

[ExtendObjectType<Mutation>]
public sealed class CharacterMutations
{
    /// <summary>
    ///     Creates a character.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="input">Command to be executed.</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>The id of the newly created character.</returns>
    public async Task<IdResponse<Character>> CreateCharacter(
        [Service] IDispatcher dispatcher,
        CreateCharacterInput input,
        CancellationToken cancellationToken = default
    )
    {
        return await dispatcher.Send(input, cancellationToken);
    }

    /// <summary>
    ///     Updates a character.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="input">Command to be executed.</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>The id of the updated character.</returns>
    public async Task<IdResponse<Character>> UpdateCharacter(
        [Service] IDispatcher dispatcher,
        UpdateCharacterInput input,
        CancellationToken cancellationToken = default
    )
    {
        return await dispatcher.Send(input, cancellationToken);
    }

    /// <summary>
    ///     Deletes a character.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="characterId">Id of the character to delete.</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>The id of the recently deleted character.</returns>
    public async Task<IdResponse<Character>> DeleteCharacter(
        [Service] IDispatcher dispatcher,
        Id<Character> characterId,
        CancellationToken cancellationToken = default
    )
    {
        return await dispatcher.Send(new DeleteEntityInput<Character>(characterId), cancellationToken);
    }
}
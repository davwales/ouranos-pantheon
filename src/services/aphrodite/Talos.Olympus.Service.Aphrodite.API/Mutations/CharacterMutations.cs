using MediatR;
using Talos.Olympus.Core.API.Mutations;
using Talos.Olympus.Core.Application.Commands.Common.DeleteEntity;
using Talos.Olympus.Core.Application.Common;
using Talos.Olympus.Core.Domain.Common;
using Talos.Olympus.Service.Aphrodite.Application.Commands.Characters.CreateCharacter;
using Talos.Olympus.Service.Aphrodite.Application.Commands.Characters.UpdateCharacter;
using Talos.Olympus.Service.Aphrodite.Domain.Characters;

namespace Talos.Olympus.Service.Aphrodite.API.Mutations;

[ExtendObjectType<Mutation>]
public sealed class CharacterMutations
{
    /// <summary>
    ///     Creates a character.
    /// </summary>
    /// <param name="mediator">
    ///     <see cref="IMediator" />
    /// </param>
    /// <param name="input">Command to be executed.</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>The id of the newly created character.</returns>
    public async Task<IdResponse<Character>> CreateCharacter(
        [Service] IMediator mediator,
        CreateCharacterInput input,
        CancellationToken cancellationToken = default
    )
    {
        return await mediator.Send(input, cancellationToken);
    }

    /// <summary>
    ///     Updates a character.
    /// </summary>
    /// <param name="mediator">
    ///     <see cref="IMediator" />
    /// </param>
    /// <param name="input">Command to be executed.</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>The id of the updated character.</returns>
    public async Task<IdResponse<Character>> UpdateCharacter(
        [Service] IMediator mediator,
        UpdateCharacterInput input,
        CancellationToken cancellationToken = default
    )
    {
        return await mediator.Send(input, cancellationToken);
    }

    /// <summary>
    ///     Deletes a character.
    /// </summary>
    /// <param name="mediator">
    ///     <see cref="IMediator" />
    /// </param>
    /// <param name="characterId">Id of the character to delete.</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>The id of the recently deleted character.</returns>
    public async Task<IdResponse<Character>> DeleteCharacter(
        [Service] IMediator mediator,
        Id<Character> characterId,
        CancellationToken cancellationToken = default
    )
    {
        return await mediator.Send(new DeleteEntityInput<Character>(characterId), cancellationToken);
    }
}
using MediatR;
using Ouranos.Pantheon.Core.API.Mutations;
using Ouranos.Pantheon.Core.Application.Commands.Common.DeleteEntity;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Aphrodite.Application.Commands.Characters.CreateCharacter;
using Ouranos.Pantheon.Service.Aphrodite.Application.Commands.Characters.UpdateCharacter;
using Ouranos.Pantheon.Service.Aphrodite.Domain.Characters;

namespace Ouranos.Pantheon.Service.Aphrodite.API.Mutations;

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
    ) => await mediator.Send(input, cancellationToken);

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
    ) => await mediator.Send(input, cancellationToken);

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
    ) => await mediator.Send(new DeleteEntityInput<Character>(characterId), cancellationToken);
}
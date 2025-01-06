using MediatR;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Service.Aphrodite.Domain.Characters;

namespace Ouranos.Pantheon.Service.Aphrodite.Application.Commands.Characters.UpdateCharacter;

public sealed class UpdateCharacterHandler : IRequestHandler<UpdateCharacterInput, IdResponse<Character>>
{
    private readonly ICrudRepository<Character> _characterRepository;
    private readonly ILogger<UpdateCharacterHandler> _logger;

    public UpdateCharacterHandler(
        ILogger<UpdateCharacterHandler> logger,
        ICrudRepository<Character> characterRepository
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(characterRepository);

        _logger = logger;
        _characterRepository = characterRepository;
    }

    public async Task<IdResponse<Character>> Handle(
        UpdateCharacterInput request,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle update character request '{@request}'.", request);
        cancellationToken.ThrowIfCancellationRequested();

        var character = await _characterRepository.Read(request.CharacterId, cancellationToken);
        character.Update(request.Name, request.Age, request.Details);
        await _characterRepository.Update(character, cancellationToken);

        _logger.LogDebug("Successfully handled update character request.");
        return new IdResponse<Character>(request.CharacterId);
    }
}
using MediatR;
using Microsoft.Extensions.Logging;
using Talos.Olympus.Core.Application.Common;
using Talos.Olympus.Core.Application.Interfaces.Common;
using Talos.Olympus.Service.Aphrodite.Domain.Characters;

namespace Talos.Olympus.Service.Aphrodite.Application.Commands.Characters.CreateCharacter;

public sealed class CreateCharacterHandler : IRequestHandler<CreateCharacterInput, IdResponse<Character>>
{
    private readonly ICrudRepository<Character> _characterRepository;
    private readonly ICreateDatabaseId<Character> _createDatabaseId;
    private readonly ILogger<CreateCharacterHandler> _logger;

    public CreateCharacterHandler(
        ILogger<CreateCharacterHandler> logger,
        ICreateDatabaseId<Character> createDatabaseId,
        ICrudRepository<Character> characterRepository
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(createDatabaseId);
        ArgumentNullException.ThrowIfNull(characterRepository);

        _logger = logger;
        _createDatabaseId = createDatabaseId;
        _characterRepository = characterRepository;
    }

    public async Task<IdResponse<Character>> Handle(
        CreateCharacterInput request,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle create character request '{@request}'.", request);
        cancellationToken.ThrowIfCancellationRequested();

        var characterId = _createDatabaseId.CreateId();
        var character = new Character(characterId, request.Name, request.Age, request.Details);
        await _characterRepository.Create(character, cancellationToken);

        _logger.LogDebug("Successfully handled create character request for character '{characterId}'.", characterId);
        return new IdResponse<Character>(characterId);
    }
}
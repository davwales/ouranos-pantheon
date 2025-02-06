using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Service.Hermes.Domain.Characters;

namespace Ouranos.Pantheon.Service.Hermes.Application.Commands.Characters.CreateCharacter;

public sealed class CreateCharacterHandler : CommandHandler<CreateCharacterInput, IdResponse<Character>>
{
    private readonly IRepository<Character> _characterRepository;
    private readonly ILogger<CreateCharacterHandler> _logger;

    public CreateCharacterHandler(
        ILogger<CreateCharacterHandler> logger,
        IRepository<Character> characterRepository
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(characterRepository);

        _logger = logger;
        _characterRepository = characterRepository;
    }

    public override async Task<IdResponse<Character>> Handle(
        CreateCharacterInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle create character command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var characterId = _characterRepository.CreateId();
        var character = new Character(characterId, command.Name, command.Age, command.Details);
        await _characterRepository.Create(character, cancellationToken);
        var response = new IdResponse<Character>(characterId);

        _logger.LogDebug("Successfully handled create character request for character '{characterId}'.", characterId);
        return response;
    }
}
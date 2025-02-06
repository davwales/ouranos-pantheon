using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Service.Hermes.Domain.Characters;

namespace Ouranos.Pantheon.Service.Hermes.Application.Commands.Characters.UpdateCharacter;

public sealed class UpdateCharacterHandler : CommandHandler<UpdateCharacterInput, IdResponse<Character>>
{
    private readonly ICrudRepository<Character> _characterRepository;
    private readonly ILogger<UpdateCharacterHandler> _logger;

    public UpdateCharacterHandler(
        ILogger<UpdateCharacterHandler> logger,
        ICrudRepository<Character> characterRepository
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(characterRepository);

        _logger = logger;
        _characterRepository = characterRepository;
    }

    public override async Task<IdResponse<Character>> Handle(
        UpdateCharacterInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle update character command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var character = await _characterRepository.Read(command.CharacterId, cancellationToken);
        character.Update(command.Name, command.Age, command.Details);
        await _characterRepository.Update(character, cancellationToken);
        var response = new IdResponse<Character>(command.CharacterId);

        _logger.LogDebug("Successfully handled update character request.");
        return response;
    }
}
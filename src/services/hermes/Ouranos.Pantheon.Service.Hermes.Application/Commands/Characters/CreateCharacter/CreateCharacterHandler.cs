using MassTransit;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Mediator;
using Ouranos.Pantheon.Service.Hermes.Domain.Characters;

namespace Ouranos.Pantheon.Service.Hermes.Application.Commands.Characters.CreateCharacter;

public sealed class CreateCharacterHandler : ICommandHandler<CreateCharacterInput, IdResponse<Character>>
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

    public async Task Consume(ConsumeContext<CreateCharacterInput> context)
    {
        _logger.LogTrace("Attempting to handle create character command '{@command}'.", context.Message);
        context.CancellationToken.ThrowIfCancellationRequested();

        var characterId = _createDatabaseId.CreateId();
        var character = new Character(characterId, context.Message.Name, context.Message.Age, context.Message.Details);
        await _characterRepository.Create(character, context.CancellationToken);

        _logger.LogDebug("Successfully handled create character request for character '{characterId}'.", characterId);
        await context.RespondAsync(new IdResponse<Character>(characterId));
    }
}
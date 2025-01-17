using MassTransit;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Mediator;
using Ouranos.Pantheon.Service.Hermes.Domain.Characters;

namespace Ouranos.Pantheon.Service.Hermes.Application.Commands.Characters.UpdateCharacter;

public sealed class UpdateCharacterHandler : ICommandHandler<UpdateCharacterInput, IdResponse<Character>>
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

    public async Task Consume(ConsumeContext<UpdateCharacterInput> context)
    {
        _logger.LogTrace("Attempting to handle update character command '{@command}'.", context.Message);
        context.CancellationToken.ThrowIfCancellationRequested();

        var character = await _characterRepository.Read(context.Message.CharacterId, context.CancellationToken);
        character.Update(context.Message.Name, context.Message.Age, context.Message.Details);
        await _characterRepository.Update(character, context.CancellationToken);

        _logger.LogDebug("Successfully handled update character request.");
        await context.RespondAsync(new IdResponse<Character>(context.Message.CharacterId));
    }
}
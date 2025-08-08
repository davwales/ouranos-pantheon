using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Service.Hermes.Domain.Assistants;

namespace Ouranos.Pantheon.Service.Hermes.Application.Commands.Assistants.UpdateAssistant;

public sealed class UpdateAssistantHandler : CommandHandler<UpdateAssistantInput, IdResponse<Assistant>>
{
    private readonly IRepository<Assistant> _assistantRepository;
    private readonly ILogger<UpdateAssistantHandler> _logger;

    public UpdateAssistantHandler(
        ILogger<UpdateAssistantHandler> logger,
        IRepository<Assistant> assistantRepository
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(assistantRepository);

        _logger = logger;
        _assistantRepository = assistantRepository;
    }

    public override async Task<IdResponse<Assistant>> Handle(
        UpdateAssistantInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle update assistant command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var assistant = await _assistantRepository.Read(command.AssistantId, cancellationToken);

        assistant.Update(
            command.Model,
            command.SystemPrompt,
            command.AssistantName,
            command.UserName,
            command.Temperature,
            command.MaxTokens,
            command.RepeatPenalty
        );

        await _assistantRepository.Update(assistant, cancellationToken);
        await _assistantRepository.SaveChanges(cancellationToken);
        var response = new IdResponse<Assistant>(command.AssistantId);

        _logger.LogDebug("Successfully handled update assistant request.");
        return response;
    }
}
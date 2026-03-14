using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Hermes.Service.Application.Interfaces.Common;
using Ouranos.Pantheon.Hermes.Service.Domain.Assistants;

namespace Ouranos.Pantheon.Hermes.Service.Application.Commands.Assistants.UpdateAssistant;

public sealed class UpdateAssistantHandler : CommandHandler<UpdateAssistantInput, IdResponse<Assistant>>
{
    private readonly IHermesUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateAssistantHandler> _logger;

    public UpdateAssistantHandler(
        ILogger<UpdateAssistantHandler> logger,
        IHermesUnitOfWork unitOfWork
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(unitOfWork);

        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public override async Task<IdResponse<Assistant>> Handle(
        UpdateAssistantInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle update assistant command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var assistant = await _unitOfWork.Assistants.Read(command.AssistantId, cancellationToken);

        assistant.Update(
            command.Model,
            command.SystemPrompt,
            command.AssistantName,
            command.UserName,
            command.Temperature,
            command.MaxTokens,
            command.RepeatPenalty
        );

        await _unitOfWork.Assistants.Update(assistant, cancellationToken);
        await _unitOfWork.SaveChanges(cancellationToken);
        var response = new IdResponse<Assistant>(command.AssistantId);

        _logger.LogDebug("Successfully handled update assistant request.");
        return response;
    }
}

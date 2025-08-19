using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Hermes.Service.Application.Interfaces.Common;
using Ouranos.Pantheon.Hermes.Service.Domain.Assistants;

namespace Ouranos.Pantheon.Hermes.Service.Application.Commands.Assistants.CreateAssistant;

public sealed class CreateAssistantHandler : CommandHandler<CreateAssistantInput, IdResponse<Assistant>>
{
    private readonly ILogger<CreateAssistantHandler> _logger;
    private readonly IHermesUnitOfWork _unitOfWork;

    public CreateAssistantHandler(
        ILogger<CreateAssistantHandler> logger,
        IHermesUnitOfWork unitOfWork
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(unitOfWork);

        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public override async Task<IdResponse<Assistant>> Handle(
        CreateAssistantInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle create assistant command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var assistant = Assistant.Create(
            _unitOfWork.Assistants.CreateId(),
            command.Model,
            command.SystemPrompt,
            command.AssistantName,
            command.UserName,
            command.Temperature,
            command.MaxTokens,
            command.RepeatPenalty
        );

        await _unitOfWork.Assistants.Create(assistant, cancellationToken);
        await _unitOfWork.SaveChanges(cancellationToken);
        var response = new IdResponse<Assistant>(assistant.Id);

        _logger.LogDebug("Successfully handled create assistant request for assistant '{assistantId}'.", assistant.Id);
        return response;
    }
}
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Service.Hermes.Domain.Assistants;

namespace Ouranos.Pantheon.Service.Hermes.Application.Commands.Assistants.CreateAssistant;

public sealed class CreateAssistantHandler : CommandHandler<CreateAssistantInput, IdResponse<Assistant>>
{
    private readonly IRepository<Assistant> _assistantRepository;
    private readonly ILogger<CreateAssistantHandler> _logger;

    public CreateAssistantHandler(
        ILogger<CreateAssistantHandler> logger,
        IRepository<Assistant> assistantRepository
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(assistantRepository);

        _logger = logger;
        _assistantRepository = assistantRepository;
    }

    public override async Task<IdResponse<Assistant>> Handle(
        CreateAssistantInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle create assistant command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var assistant = new Assistant(
            _assistantRepository.CreateId(),
            command.Model,
            command.SystemPrompt,
            command.AssistantName,
            command.UserName
        );

        await _assistantRepository.Create(assistant, cancellationToken);
        var response = new IdResponse<Assistant>(assistant.Id);

        _logger.LogDebug("Successfully handled create assistant request for assistant '{assistantId}'.", assistant.Id);
        return response;
    }
}
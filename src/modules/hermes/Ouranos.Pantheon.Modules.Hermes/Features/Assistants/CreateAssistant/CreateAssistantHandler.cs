using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.CreateAssistant.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Assistants;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Extensions;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.CreateAssistant;

public sealed class CreateAssistantHandler : IPantheonHandler<CreateAssistantInput, IdResponse<Assistant>>
{
    private readonly ILogger<CreateAssistantHandler> _logger;
    private readonly HermesDbContext _dbContext;

    public CreateAssistantHandler(
        ILogger<CreateAssistantHandler> logger,
        HermesDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<IdResponse<Assistant>> Handle(
        CreateAssistantInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle create assistant command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var assistant = Assistant.Create(
            DatabaseExtensions.CreateId<Assistant>(),
            command.Model,
            command.SystemPrompt,
            command.AssistantName,
            command.UserName,
            command.Temperature,
            command.MaxTokens,
            command.RepeatPenalty
        );

        await _dbContext.Assistants.AddAsync(assistant, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var response = new IdResponse<Assistant>(assistant.Id);

        _logger.LogDebug("Successfully handled create assistant request for assistant '{assistantId}'.", assistant.Id);
        return response;
    }
}
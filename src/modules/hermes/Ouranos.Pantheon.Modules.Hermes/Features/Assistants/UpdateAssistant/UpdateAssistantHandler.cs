using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.UpdateAssistant.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Assistants;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.UpdateAssistant;

public sealed class UpdateAssistantHandler : CommandHandler<UpdateAssistantInput, IdResponse<Assistant>>
{
    private readonly HermesDbContext _dbContext;
    private readonly ILogger<UpdateAssistantHandler> _logger;

    public UpdateAssistantHandler(
        ILogger<UpdateAssistantHandler> logger,
        HermesDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public override async Task<IdResponse<Assistant>> Handle(
        UpdateAssistantInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle update assistant command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var assistant = await _dbContext.Assistants
            .FirstOrDefaultAsync(a => a.Id == command.AssistantId, cancellationToken);

        Guard.Against.NotFound(command.AssistantId, assistant);

        assistant.Update(
            command.Model,
            command.SystemPrompt,
            command.AssistantName,
            command.UserName,
            command.Temperature,
            command.MaxTokens,
            command.RepeatPenalty
        );

        _dbContext.Assistants.Update(assistant);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var response = new IdResponse<Assistant>(command.AssistantId);

        _logger.LogDebug("Successfully handled update assistant request.");
        return response;
    }
}

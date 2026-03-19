using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.GetAssistant.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.GetAssistant;

public sealed class GetAssistantHandler : QueryHandler<GetAssistantInput, GetAssistantResponse>
{
    private readonly HermesDbContext _dbContext;
    private readonly ILogger<GetAssistantHandler> _logger;

    public GetAssistantHandler(
        ILogger<GetAssistantHandler> logger,
        HermesDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public override async Task<GetAssistantResponse> Handle(
        GetAssistantInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get assistant query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var assistant = await _dbContext.Assistants
            .FirstOrDefaultAsync(a => a.Id == query.AssistantId, cancellationToken);

        Guard.Against.NotFound(query.AssistantId, assistant);

        _logger.LogDebug("Successfully handled get assistant request.");
        return new GetAssistantResponse(
            assistant.Id,
            assistant.Model,
            assistant.SystemPrompt,
            assistant.AssistantName,
            assistant.UserName,
            assistant.Temperature,
            assistant.MaxTokens,
            assistant.RepeatPenalty
        );
    }
}

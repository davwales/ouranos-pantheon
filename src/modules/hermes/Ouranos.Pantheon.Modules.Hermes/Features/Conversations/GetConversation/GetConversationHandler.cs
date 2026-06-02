using Ardalis.GuardClauses;
using Flagsmith;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GetConversation.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Shared.Application;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GetConversation;

public sealed class GetConversationHandler(
    ILogger<GetConversationHandler> logger,
    HermesDbContext dbContext,
    IFlagsmithClient flagsmith
) : IPantheonHandler<GetConversationInput, GetConversationResponse>
{
    private readonly HermesDbContext _dbContext = Guard.Against.Null(dbContext);
    private readonly IFlagsmithClient _flagsmith = Guard.Against.Null(flagsmith);
    private readonly ILogger<GetConversationHandler> _logger = Guard.Against.Null(logger);

    public async Task<GetConversationResponse> Handle(
        GetConversationInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get conversation query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var conversation = await _dbContext
            .Conversations.Include(c => c.Persona)
            .Include(c => c.ModelConfig)
            .Include(c => c.Traits)
            .Include(c => c.Folder)
            .Include(c => c.Messages.OrderBy(m => m.SortOrder))
            .FirstOrDefaultAsync(c => c.Id == query.ConversationId, cancellationToken);

        Guard.Against.NotFound(query.ConversationId, conversation);

        var flags = await _flagsmith.GetEnvironmentFlags();
        var isPublicMode = await flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode);

        if (isPublicMode && (!conversation.IsPublic || conversation.Folder?.IsPublic == false))
        {
            Guard.Against.NotFound(query.ConversationId, (Conversation?)null);
        }

        _logger.LogDebug("Successfully handled get conversation request.");

        GetConversationTokenUsageResponse? tokenUsage = null;
        if (
            conversation.InputTokenCount.HasValue
            && conversation.OutputTokenCount.HasValue
            && conversation.TotalTokenCount.HasValue
        )
        {
            tokenUsage = new GetConversationTokenUsageResponse(
                conversation.InputTokenCount.Value,
                conversation.OutputTokenCount.Value,
                conversation.TotalTokenCount.Value
            );
        }

        return new GetConversationResponse(
            conversation.Id,
            conversation.Name,
            conversation.IsPublic,
            conversation.FolderId,
            new GetConversationPersonaResponse(
                conversation.Persona.Id,
                conversation.Persona.Name,
                conversation.Persona.Description,
                conversation.Persona.Personality,
                conversation.Persona.Scenario
            ),
            new GetConversationModelResponse(
                conversation.ModelConfig.Id,
                conversation.ModelConfig.Name,
                conversation.ModelConfig.ModelIdentifier,
                conversation.ModelConfig.SystemPrompt,
                conversation.ModelConfig.Temperature,
                conversation.ModelConfig.MaxTokens,
                conversation.ModelConfig.RepeatPenalty,
                conversation.ModelConfig.ContextWindow
            ),
            [
                .. conversation.Traits.Select(t => new GetConversationTraitResponse(
                    t.Id,
                    t.Name,
                    t.Content
                )),
            ],
            [
                .. conversation.Messages.Select(m => new GetConversationMessageResponse(
                    m.Id,
                    m.Content,
                    m.Role,
                    m.SortOrder
                )),
            ],
            tokenUsage,
            conversation.CreatedAt,
            conversation.UpdatedAt
        );
    }
}

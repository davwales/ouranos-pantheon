using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Extensions;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.UpdateConversation.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.UpdateConversation;

public sealed class UpdateConversationHandler : IPantheonHandler<UpdateConversationInput, IdResponse<Conversation>>
{
    private readonly ILogger<UpdateConversationHandler> _logger;
    private readonly HermesDbContext _dbContext;

    public UpdateConversationHandler(
        ILogger<UpdateConversationHandler> logger,
        HermesDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<IdResponse<Conversation>> Handle(
        UpdateConversationInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle update conversation command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var conversation = await _dbContext.Conversations
            .Include(c => c.Traits)
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == command.ConversationId, cancellationToken);

        Guard.Against.NotFound(command.ConversationId, conversation);

        var traits = await _dbContext.Traits
            .Where(t => command.TraitIds.Contains(t.Id))
            .ToListAsync(cancellationToken);

        var newMessages = command.Messages
            .Select((m, index) => Message.Create(
                DatabaseExtensions.CreateId<Message>(),
                command.ConversationId,
                m.Content,
                m.Role,
                index
            ));

        conversation.Update(command.Name, command.PersonaId, command.ModelConfigId, newMessages, traits, command.IsPublic);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully handled update conversation request for conversation '{conversationId}'.", conversation.Id);
        return new IdResponse<Conversation>(conversation.Id);
    }
}

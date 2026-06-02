using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.DeleteConversation.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Application.Common;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.DeleteConversation;

public sealed class DeleteConversationHandler
    : IPantheonHandler<DeleteConversationInput, IdResponse<Conversation>>
{
    private readonly HermesDbContext _dbContext;
    private readonly ILogger<DeleteConversationHandler> _logger;

    public DeleteConversationHandler(
        ILogger<DeleteConversationHandler> logger,
        HermesDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<IdResponse<Conversation>> Handle(
        DeleteConversationInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle delete conversation command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var conversation = await _dbContext
            .Conversations.Include(c => c.Folder)
            .FirstOrDefaultAsync(c => c.Id == command.ConversationId, cancellationToken);

        Guard.Against.NotFound(command.ConversationId, conversation);

        _dbContext.Conversations.Remove(conversation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully handled delete conversation request.");
        return new IdResponse<Conversation>(command.ConversationId);
    }
}

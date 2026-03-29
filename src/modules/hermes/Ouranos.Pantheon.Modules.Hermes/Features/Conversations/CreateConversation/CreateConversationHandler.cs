using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CreateConversation.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Extensions;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CreateConversation;

public sealed class CreateConversationHandler : IPantheonHandler<CreateConversationInput, CreateConversationResponse>
{
    private readonly ILogger<CreateConversationHandler> _logger;
    private readonly HermesDbContext _dbContext;

    public CreateConversationHandler(
        ILogger<CreateConversationHandler> logger,
        HermesDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<CreateConversationResponse> Handle(
        CreateConversationInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle create conversation command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var conversationId = DatabaseExtensions.CreateId<Conversation>();

        var messages = command.Messages
            .Select((m, index) => Message.Create(
                DatabaseExtensions.CreateId<Message>(),
                conversationId,
                m.Content,
                m.Role,
                index
            ))
            .ToList();

        var traits = command.TraitIds.Length > 0
            ? await _dbContext.Traits
                .Where(t => command.TraitIds.Contains(t.Id))
                .ToListAsync(cancellationToken)
            : [];

        var conversation = Conversation.Create(conversationId, command.PersonaId, command.ModelConfigId, messages, traits, command.Name, command.IsPublic);

        await _dbContext.Conversations.AddAsync(conversation, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully handled create conversation request for conversation '{conversationId}'.", conversation.Id);
        return new CreateConversationResponse(conversation.Id, conversation.Name);
    }
}

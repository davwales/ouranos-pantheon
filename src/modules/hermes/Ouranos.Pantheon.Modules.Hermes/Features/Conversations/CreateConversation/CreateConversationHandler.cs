using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CreateConversation.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateConversationName.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CreateConversation;

public sealed class CreateConversationHandler
    : IPantheonHandler<CreateConversationInput, CreateConversationResponse>
{
    private readonly ILogger<CreateConversationHandler> _logger;
    private readonly HermesDbContext _dbContext;
    private readonly IMessageBus _bus;
    private readonly IOptions<HermesOptions> _options;

    public CreateConversationHandler(
        ILogger<CreateConversationHandler> logger,
        HermesDbContext dbContext,
        IMessageBus bus,
        IOptions<HermesOptions> options
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);
        Guard.Against.Null(bus);
        Guard.Against.Null(options);

        _logger = logger;
        _dbContext = dbContext;
        _bus = bus;
        _options = options;
    }

    public async Task<CreateConversationResponse> Handle(
        CreateConversationInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle create conversation command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        Guard.Against.Null(command.Messages, nameof(command.Messages));

        var conversationId = DatabaseExtensions.CreateId<Conversation>();

        var messages = command
            .Messages.Select(
                (m, index) =>
                    Message.Create(
                        DatabaseExtensions.CreateId<Message>(),
                        conversationId,
                        m.Content,
                        m.Role,
                        index,
                        m.Parts?.Select(MessagePartMapping.ToEntity)
                    )
            )
            .ToList();

        var traits = command.TraitIds is { Length: > 0 }
            ? await _dbContext
                .Traits.Where(t => command.TraitIds.Contains(t.Id))
                .ToListAsync(cancellationToken)
            : [];

        var loras = await LoraValidation.LoadByIdsAsync(
            _dbContext,
            command.LoraIds,
            cancellationToken
        );

        var folder = await FolderValidation.ValidateFolderExistsAsync(
            _dbContext,
            command.FolderId,
            cancellationToken
        );

        await ToolSetValidation.ValidateToolSetExistsAsync(
            _dbContext,
            command.ToolSetId,
            cancellationToken
        );

        var conversation = Conversation.Create(
            conversationId,
            command.PersonaId,
            command.ModelConfigId,
            messages,
            traits,
            command.Name,
            command.IsPublic,
            folderId: command.FolderId,
            folder: folder,
            reasoningEffort: command.ReasoningEffort,
            loras: loras,
            toolSetId: command.ToolSetId
        );

        if (
            command.InputTokenCount.HasValue
            && command.OutputTokenCount.HasValue
            && command.TotalTokenCount.HasValue
        )
        {
            conversation.RecordTokenUsage(
                command.InputTokenCount.Value,
                command.OutputTokenCount.Value,
                command.TotalTokenCount.Value
            );
        }

        await _dbContext.Conversations.AddAsync(conversation, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (
            string.IsNullOrWhiteSpace(command.Name)
            && messages.Count > 0
            && _options.Value.IsConversationNameGenerationEnabled
        )
        {
            await RequestNameAsync(conversation.Id);
        }

        _logger.LogDebug(
            "Successfully handled create conversation request for conversation '{conversationId}'.",
            conversation.Id
        );
        return new CreateConversationResponse(conversation.Id, conversation.Name);
    }

    private async Task RequestNameAsync(Id<Conversation> conversationId)
    {
        try
        {
            await _bus.PublishAsync(new GenerateConversationNameRequested(conversationId));
        }
        catch (Exception ex)
        {
            // The conversation is already saved, so a lost naming request only leaves the default
            // name rather than failing the save.
            _logger.LogWarning(
                ex,
                "Failed to request a name for conversation '{conversationId}'.",
                conversationId
            );
        }
    }
}

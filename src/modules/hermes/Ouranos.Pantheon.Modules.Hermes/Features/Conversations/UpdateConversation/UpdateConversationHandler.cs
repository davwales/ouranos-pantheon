using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.UpdateConversation.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.UpdateConversation;

public sealed class UpdateConversationHandler
    : IPantheonHandler<UpdateConversationInput, IdResponse<Conversation>>
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

        Guard.Against.Null(command.TraitIds, nameof(command.TraitIds));

        var conversation = await _dbContext
            .Conversations.Include(c => c.Traits)
            .Include(c => c.Loras)
            .Include(c => c.Messages)
            .Include(c => c.Folder)
            .FirstOrDefaultAsync(c => c.Id == command.ConversationId, cancellationToken);

        Guard.Against.NotFound(command.ConversationId, conversation);

        if (command.FolderId is not null && command.FolderId != conversation.FolderId)
        {
            await FolderValidation.ValidateFolderExistsAsync(
                _dbContext,
                command.FolderId,
                cancellationToken
            );
        }

        if (command.PersonaId != conversation.PersonaId)
        {
            await PersonaValidation.ValidatePersonaExistsAsync(
                _dbContext,
                command.PersonaId,
                cancellationToken
            );
        }

        if (command.ModelConfigId != conversation.ModelConfigId)
        {
            await ModelConfigValidation.ValidateModelConfigExistsAsync(
                _dbContext,
                command.ModelConfigId,
                cancellationToken
            );
        }

        var traits = await TraitValidation.LoadByIdsAsync(
            _dbContext,
            command.TraitIds,
            cancellationToken
        );

        var loras = await LoraValidation.LoadByIdsAsync(
            _dbContext,
            command.LoraIds,
            cancellationToken
        );

        await ToolSetValidation.ValidateToolSetExistsAsync(
            _dbContext,
            command.ToolSetId,
            cancellationToken
        );

        var newMessages = command.Messages?.Select(
            (m, index) =>
                Message.Create(
                    DatabaseExtensions.CreateId<Message>(),
                    command.ConversationId,
                    m.Content,
                    m.Role,
                    index,
                    m.Parts?.Select(MessagePartMapping.ToEntity)
                )
        );

        conversation.Update(
            command.Name,
            command.PersonaId,
            command.ModelConfigId,
            newMessages,
            traits,
            command.IsPublic,
            command.FolderId,
            command.ReasoningEffort,
            loras,
            command.ToolSetId
        );

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Successfully handled update conversation request for conversation '{conversationId}'.",
            conversation.Id
        );
        return new IdResponse<Conversation>(conversation.Id);
    }
}

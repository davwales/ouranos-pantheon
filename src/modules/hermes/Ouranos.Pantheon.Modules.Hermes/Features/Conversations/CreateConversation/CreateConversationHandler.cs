using System.ClientModel;
using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CreateConversation.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CreateConversation;

public sealed class CreateConversationHandler
    : IPantheonHandler<CreateConversationInput, CreateConversationResponse>
{
    private const int MaxGeneratedNameLength = 60;

    private readonly ILogger<CreateConversationHandler> _logger;
    private readonly HermesDbContext _dbContext;
    private readonly IOuranosMachineLearningClient _mlClient;
    private readonly IOptions<HermesOptions> _options;

    public CreateConversationHandler(
        ILogger<CreateConversationHandler> logger,
        HermesDbContext dbContext,
        IOuranosMachineLearningClient mlClient,
        IOptions<HermesOptions> options
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);
        Guard.Against.Null(mlClient);
        Guard.Against.Null(options);

        _logger = logger;
        _dbContext = dbContext;
        _mlClient = mlClient;
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

        var name = string.IsNullOrWhiteSpace(command.Name)
            ? await GenerateNameAsync(command, cancellationToken)
            : command.Name;

        var conversation = Conversation.Create(
            conversationId,
            command.PersonaId,
            command.ModelConfigId,
            messages,
            traits,
            name,
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

        _logger.LogDebug(
            "Successfully handled create conversation request for conversation '{conversationId}'.",
            conversation.Id
        );
        return new CreateConversationResponse(conversation.Id, conversation.Name);
    }

    private async Task<string?> GenerateNameAsync(
        CreateConversationInput command,
        CancellationToken cancellationToken
    )
    {
        var options = _options.Value;
        if (command.Messages.Count == 0 || !options.IsConversationNameGenerationEnabled)
        {
            return null;
        }

        try
        {
            List<ResponseInputItemDto> input =
            [
                .. command.Messages.Select(m => new MessageInputItemDto(
                    new MessageDto(m.Content, MapRole(m.Role))
                )),
            ];

            if (command.Messages[^1].Role != Role.User)
            {
                input.Add(
                    new MessageInputItemDto(
                        new MessageDto("Generate a name using the prior content.", RoleDto.User)
                    )
                );
            }

            var result = await _mlClient.GenerateResponseAsync(
                options.ConversationNameModel,
                options.ConversationNameSystemPrompt,
                input,
                reasoningEffort: options.ConversationNameReasoningEffort,
                cancellationToken: cancellationToken
            );

            var trimmed = result.Content?.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                return null;
            }

            return trimmed.Length > MaxGeneratedNameLength
                ? trimmed[..MaxGeneratedNameLength]
                : trimmed;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to auto-generate conversation name; falling back to default."
            );
            return null;
        }
        catch (ClientResultException ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to auto-generate conversation name; falling back to default."
            );
            return null;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to auto-generate conversation name; falling back to default."
            );
            return null;
        }
    }

    private static RoleDto MapRole(Role role)
    {
        return role switch
        {
            Role.System => RoleDto.System,
            Role.Assistant => RoleDto.Assistant,
            Role.User => RoleDto.User,
            Role.Summary => RoleDto.Assistant,
            _ => throw new ArgumentException($"Unsupported role: {role}", nameof(role)),
        };
    }
}

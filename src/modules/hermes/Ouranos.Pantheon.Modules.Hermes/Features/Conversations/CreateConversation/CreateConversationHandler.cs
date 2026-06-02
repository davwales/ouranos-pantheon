using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CreateConversation.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Extensions;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Dtos;

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
                        index
                    )
            )
            .ToList();

        var traits = command.TraitIds is { Length: > 0 }
            ? await _dbContext
                .Traits.Where(t => command.TraitIds.Contains(t.Id))
                .ToListAsync(cancellationToken)
            : [];

        var folder = await FolderValidation.ValidateFolderExistsAsync(
            _dbContext,
            command.FolderId,
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
            folder: folder
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
            var messages = new List<MessageDto>
            {
                new(_options.Value.ConversationNameSystemPrompt, RoleDto.System),
            };

            messages.AddRange(
                command.Messages.Select(m => new MessageDto(m.Content, MapRole(m.Role)))
            );

            var lastMessage = messages.Last();
            if (lastMessage.Role != RoleDto.User)
            {
                messages.Add(
                    new MessageDto("Generate a name using the prior content.", RoleDto.User)
                );
            }

            var result = await _mlClient.GenerateChatCompletionAsync(
                options.ConversationNameModel,
                messages,
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

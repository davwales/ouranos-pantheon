using System.ClientModel;
using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateConversationName.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateConversationName;

/// <summary>
/// Names a newly saved conversation from its messages after the create request has returned,
/// so saving never waits on the model. Naming is best-effort: a failure keeps the default name.
/// </summary>
public sealed class GenerateConversationNameConsumer(
    ILogger<GenerateConversationNameConsumer> logger,
    HermesDbContext dbContext,
    IOuranosMachineLearningClient mlClient,
    IOptions<HermesOptions> options
) : IPantheonHandler<GenerateConversationNameRequested>
{
    private const int MaxGeneratedNameLength = 60;

    private readonly ILogger<GenerateConversationNameConsumer> _logger = Guard.Against.Null(logger);
    private readonly HermesDbContext _dbContext = Guard.Against.Null(dbContext);
    private readonly IOuranosMachineLearningClient _mlClient = Guard.Against.Null(mlClient);
    private readonly HermesOptions _options = Guard.Against.Null(options).Value;

    public async Task Handle(
        GenerateConversationNameRequested message,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Processing conversation name request '{@message}'.", message);
        cancellationToken.ThrowIfCancellationRequested();

        var conversation = await _dbContext
            .Conversations.Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == message.ConversationId, cancellationToken);

        if (conversation is null || !conversation.HasDefaultName)
        {
            _logger.LogDebug(
                "Skipped naming conversation '{conversationId}': it was deleted or renamed.",
                message.ConversationId
            );
            return;
        }

        var name = await GenerateNameAsync(conversation, cancellationToken);
        if (name is null || !conversation.ApplyGeneratedName(name))
        {
            return;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Successfully named conversation '{conversationId}'.",
            message.ConversationId
        );
    }

    private async Task<string?> GenerateNameAsync(
        Conversation conversation,
        CancellationToken cancellationToken
    )
    {
        var messages = conversation.Messages.OrderBy(m => m.SortOrder).ToList();
        if (messages.Count == 0 || !_options.IsConversationNameGenerationEnabled)
        {
            return null;
        }

        List<ResponseInputItemDto> input =
        [
            .. messages.Select(m => new MessageInputItemDto(
                new MessageDto(m.Content, m.Role.ToRoleDto())
            )),
        ];

        if (messages[^1].Role != Role.User)
        {
            input.Add(
                new MessageInputItemDto(
                    new MessageDto("Generate a name using the prior content.", RoleDto.User)
                )
            );
        }

        try
        {
            var result = await _mlClient.GenerateResponseAsync(
                _options.ConversationNameModel,
                _options.ConversationNameSystemPrompt,
                input,
                reasoningEffort: _options.ConversationNameReasoningEffort,
                cancellationToken: cancellationToken
            );

            var trimmed = result.Content?.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                return null;
            }

            return trimmed.Length > MaxGeneratedNameLength
                ? trimmed[..MaxGeneratedNameLength].TrimEnd()
                : trimmed;
        }
        catch (Exception ex)
            when (ex is HttpRequestException or ClientResultException
                || (ex is OperationCanceledException && !cancellationToken.IsCancellationRequested)
            )
        {
            // A model timeout also surfaces as a cancellation; only the caller's token means the
            // host is stopping, which must propagate.
            _logger.LogWarning(
                ex,
                "Failed to generate a name for conversation '{conversationId}'; keeping the default.",
                conversation.Id
            );
            return null;
        }
    }
}

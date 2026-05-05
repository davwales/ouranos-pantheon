using System.Runtime.CompilerServices;
using System.Text;
using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CompactConversation.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Shared.Extensions;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Dtos;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CompactConversation;

public sealed class CompactConversationHandler
    : IPantheonStreamHandler<CompactConversationInput, CompactConversationResponse>
{
    private readonly ILogger<CompactConversationHandler> _logger;
    private readonly IOuranosMachineLearningClient _mlClient;
    private readonly IDbContextFactory<HermesDbContext> _dbContextFactory;

    private const string SummaryPromptTemplate =
        """
        You are summarizing a conversation for context compaction.

        The conversation is with {PersonaName}: {PersonaDescription}

        Summarize the following conversation history concisely, preserving:
        - Key topics discussed and decisions made
        - Important facts, preferences, or constraints established
        - The current state of any ongoing tasks or problems
        - Any unresolved questions or open items

        Be concise but thorough. The summary will replace the full conversation history for future context.
        """;

    public CompactConversationHandler(
        ILogger<CompactConversationHandler> logger,
        IOuranosMachineLearningClient mlClient,
        IDbContextFactory<HermesDbContext> dbContextFactory
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(mlClient);
        Guard.Against.Null(dbContextFactory);

        _logger = logger;
        _mlClient = mlClient;
        _dbContextFactory = dbContextFactory;
    }

    public async IAsyncEnumerable<CompactConversationResponse> Handle(
        CompactConversationInput command,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle compact conversation command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        Guard.Against.Null(command.Messages, nameof(command.Messages));

        var lastSummaryIndex = command.Messages.FindLastIndex(m => m.Role == Role.Summary);

        var messagesToSummarize = lastSummaryIndex >= 0
            ? command.Messages.Skip(lastSummaryIndex + 1)
            : command.Messages;

        var userAndAssistantMessages = messagesToSummarize
            .Where(m => m.Role is Role.User or Role.Assistant)
            .ToList();

        if (userAndAssistantMessages.Count == 0)
        {
            throw new InvalidOperationException(
                "Cannot compact a conversation with no user or assistant messages to summarize."
            );
        }

        var summaryPrompt = ComposeSummaryPrompt(command, userAndAssistantMessages);
        var buffer = new StringBuilder();
        ChatCompletionUsage? tokenUsage = null;

        await foreach (
            var chunk in _mlClient.StreamChatCompletionAsync(
                command.ModelIdentifier,
                [
                    new MessageDto(summaryPrompt, RoleDto.System),
                    new MessageDto("Provide the summary now.", RoleDto.User)
                ],
                temperature: 0.3f,
                maxTokens: 1024,
                cancellationToken: cancellationToken
            )
        )
        {
            if (chunk.Text is not null)
            {
                buffer.Append(chunk.Text);
                yield return new CompactContentChunkResponse(chunk.Text);
            }

            if (chunk.Usage is not null)
            {
                tokenUsage = chunk.Usage;
            }
        }

        var summary = buffer.ToString().Trim();
        var summaryMessageId = await PersistSummary(command, summary, tokenUsage, cancellationToken);

        if (tokenUsage is not null)
        {
            yield return new CompactUsageChunkResponse(
                0,
                tokenUsage.OutputTokens,
                tokenUsage.OutputTokens
            );
        }

        yield return new CompactCompleteResponse(summaryMessageId);

        _logger.LogDebug("Successfully compacted conversation.");
    }

    private async Task<Id<Message>?> PersistSummary(
        CompactConversationInput command,
        string summary,
        ChatCompletionUsage? tokenUsage,
        CancellationToken cancellationToken
    )
    {
        if (command.ConversationId is null)
        {
            return null;
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var conversation = await dbContext.Conversations
            .FirstOrDefaultAsync(c => c.Id == command.ConversationId, cancellationToken);

        Guard.Against.NotFound(command.ConversationId.Value, conversation);

        var existingCount = await dbContext.Messages
            .CountAsync(m => m.ConversationId == command.ConversationId, cancellationToken);

        var summaryMessage = Message.Create(
            DatabaseExtensions.CreateId<Message>(),
            command.ConversationId.Value,
            summary,
            Role.Summary,
            existingCount
        );

        await dbContext.Messages.AddAsync(summaryMessage, cancellationToken);

        if (tokenUsage is not null)
        {
            conversation.RecordTokenUsage(
                0,
                tokenUsage.OutputTokens,
                tokenUsage.OutputTokens
            );
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return summaryMessage.Id;
    }

    internal static string ComposeSummaryPrompt(
        CompactConversationInput command,
        IEnumerable<CompactConversationMessageInput> messages
    )
    {
        var prompt = SummaryPromptTemplate
            .Replace("{PersonaName}", command.PersonaName)
            .Replace("{PersonaDescription}", command.PersonaDescription);

        var builder = new StringBuilder(prompt);

        builder.AppendLine("Conversation history:");
        foreach (var message in messages)
        {
            var roleLabel = message.Role switch
            {
                Role.Assistant => command.PersonaName,
                _ => message.Role.ToString()
            };
            builder.AppendLine($"{roleLabel}: {message.Content}");
        }

        return builder.ToString();
    }
}
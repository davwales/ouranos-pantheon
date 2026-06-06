using System.Runtime.CompilerServices;
using System.Text;
using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CompactConversation.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
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
    private readonly IOptions<HermesOptions> _options;

    public CompactConversationHandler(
        ILogger<CompactConversationHandler> logger,
        IOuranosMachineLearningClient mlClient,
        IDbContextFactory<HermesDbContext> dbContextFactory,
        IOptions<HermesOptions> options
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(mlClient);
        Guard.Against.Null(dbContextFactory);
        Guard.Against.Null(options);

        _logger = logger;
        _mlClient = mlClient;
        _dbContextFactory = dbContextFactory;
        _options = options;
    }

    public async IAsyncEnumerable<CompactConversationResponse> Handle(
        CompactConversationInput command,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace(
            "Attempting to handle compact conversation command '{@command}'.",
            command
        );
        cancellationToken.ThrowIfCancellationRequested();

        if (command.ConversationId is not null)
        {
            await using var authContext = await _dbContextFactory.CreateDbContextAsync(
                cancellationToken
            );

            var conversation = await authContext
                .Conversations.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == command.ConversationId, cancellationToken);

            Guard.Against.NotFound(command.ConversationId.Value, conversation);
        }

        Guard.Against.Null(command.Messages, nameof(command.Messages));

        var lastSummaryIndex = command.Messages.FindLastIndex(m => m.Role == Role.Summary);

        var messagesToSummarize =
            lastSummaryIndex >= 0 ? command.Messages.Skip(lastSummaryIndex + 1) : command.Messages;

        var userAndAssistantMessages = messagesToSummarize
            .Where(m => m.Role is Role.User or Role.Assistant)
            .ToList();

        if (userAndAssistantMessages.Count == 0)
        {
            throw new InvalidOperationException(
                "Cannot compact a conversation with no user or assistant messages to summarize."
            );
        }

        var options = _options.Value;
        var summaryPrompt = ComposeSummaryPrompt(
            command,
            userAndAssistantMessages,
            options.EffectiveCompactionSummaryPrompt
        );
        var buffer = new StringBuilder();
        ChatCompletionUsage? tokenUsage = null;

        await foreach (
            var chunk in _mlClient.StreamChatCompletionAsync(
                command.ModelIdentifier,
                [
                    new MessageDto(summaryPrompt, RoleDto.System),
                    new MessageDto("Provide the summary now.", RoleDto.User),
                ],
                temperature: options.EffectiveCompactionTemperature,
                maxTokens: options.EffectiveCompactionMaxTokens,
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
        var summaryMessageId = await PersistSummary(
            command,
            summary,
            tokenUsage,
            cancellationToken
        );

        if (tokenUsage is not null)
        {
            yield return new CompactUsageChunkResponse(
                tokenUsage.InputTokens,
                tokenUsage.OutputTokens,
                tokenUsage.TotalTokens
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

        var conversation = await dbContext.Conversations.FirstOrDefaultAsync(
            c => c.Id == command.ConversationId,
            cancellationToken
        );

        Guard.Against.NotFound(command.ConversationId.Value, conversation);

        var existingCount = await dbContext.Messages.CountAsync(
            m => m.ConversationId == command.ConversationId,
            cancellationToken
        );

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
                tokenUsage.InputTokens,
                tokenUsage.OutputTokens,
                tokenUsage.TotalTokens
            );
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return summaryMessage.Id;
    }

    internal static string ComposeSummaryPrompt(
        CompactConversationInput command,
        IEnumerable<CompactConversationMessageInput> messages,
        string summaryPromptTemplate
    )
    {
        var prompt = summaryPromptTemplate
            .Replace("{PersonaName}", command.PersonaName)
            .Replace("{PersonaDescription}", command.PersonaDescription);

        var builder = new StringBuilder(prompt);

        builder.AppendLine("Conversation history:");
        foreach (var message in messages)
        {
            var roleLabel = message.Role switch
            {
                Role.Assistant => command.PersonaName,
                _ => message.Role.ToString(),
            };
            builder.AppendLine($"{roleLabel}: {message.Content}");
        }

        return builder.ToString();
    }
}

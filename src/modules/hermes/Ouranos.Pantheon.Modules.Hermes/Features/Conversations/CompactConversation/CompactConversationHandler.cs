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
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CompactConversation;

public sealed class CompactConversationHandler
    : IPantheonStreamHandler<CompactConversationInput, CompactConversationResponse>
{
    internal const string NothingToCompactMessage = "Nothing to compact.";

    internal const string CompactionFailedMessage = "Compaction failed. Please try again.";

    private const string TruncationMarker = "…";

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
            _logger.LogWarning(
                "Conversation '{conversationId}' has no user or assistant messages to compact.",
                command.ConversationId
            );
            yield return new CompactErrorResponse(NothingToCompactMessage);
            yield break;
        }

        var options = _options.Value;
        var summaryPrompt = ComposeSummaryPrompt(
            command,
            userAndAssistantMessages,
            options.EffectiveCompactionSummaryPrompt,
            options.EffectiveCompactionMaxInputCharacters
        );
        var buffer = new StringBuilder();
        ChatCompletionUsage? tokenUsage = null;

        var stream = _mlClient
            .StreamResponseAsync(
                command.ModelIdentifier,
                summaryPrompt,
                [new MessageInputItemDto(new MessageDto("Provide the summary now.", RoleDto.User))],
                [],
                options.EffectiveCompactionTemperature,
                options.EffectiveCompactionMaxTokens,
                options.CompactionReasoningEffort,
                cancellationToken
            )
            .GetAsyncEnumerator(cancellationToken);

        await using (stream)
        {
            while (true)
            {
                var (hasNext, error) = await TryMoveNextAsync(stream);

                if (error is not null)
                {
                    _logger.LogError(
                        error,
                        "Compaction of conversation '{conversationId}' failed while streaming.",
                        command.ConversationId
                    );
                    yield return new CompactErrorResponse(CompactionFailedMessage);
                    yield break;
                }

                if (!hasNext)
                {
                    break;
                }

                var chunk = stream.Current;

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
        }

        var summary = buffer.ToString().Trim();

        if (string.IsNullOrWhiteSpace(summary))
        {
            _logger.LogWarning(
                "Compaction of conversation '{conversationId}' produced no summary after {outputTokens} output tokens.",
                command.ConversationId,
                tokenUsage?.OutputTokens
            );
            yield return new CompactErrorResponse(CompactionFailedMessage);
            yield break;
        }

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
            conversation.RecordTokenUsage(0, tokenUsage.OutputTokens, tokenUsage.OutputTokens);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return summaryMessage.Id;
    }

    internal static string ComposeSummaryPrompt(
        CompactConversationInput command,
        IEnumerable<CompactConversationMessageInput> messages,
        string summaryPromptTemplate,
        int maxInputCharacters
    )
    {
        var prompt = summaryPromptTemplate
            .Replace("{PersonaName}", command.PersonaName)
            .Replace("{PersonaDescription}", command.PersonaDescription);

        var lines = messages
            .Select(message =>
            {
                var roleLabel = message.Role switch
                {
                    Role.Assistant => command.PersonaName,
                    _ => message.Role.ToString(),
                };
                return $"{roleLabel}: {message.Content}";
            })
            .ToList();

        var keptLines = new List<string>();
        var remaining = maxInputCharacters;

        for (var i = lines.Count - 1; i >= 0; i--)
        {
            var line = lines[i];

            if (line.Length <= remaining)
            {
                keptLines.Add(line);
                remaining -= line.Length;
                continue;
            }

            if (keptLines.Count == 0)
            {
                keptLines.Add(Truncate(line, maxInputCharacters));
            }

            break;
        }

        keptLines.Reverse();

        var builder = new StringBuilder(prompt);

        builder.AppendLine("Conversation history:");
        if (keptLines.Count < lines.Count)
        {
            builder.AppendLine("(Earlier messages omitted.)");
        }

        foreach (var line in keptLines)
        {
            builder.AppendLine(line);
        }

        return builder.ToString();
    }

    private static string Truncate(string value, int maxCharacters)
    {
        var length = Math.Max(0, maxCharacters - TruncationMarker.Length);
        return $"{value[..length]}{TruncationMarker}";
    }

    // C# forbids yield inside a try with a catch, so stream failures are returned instead of
    // thrown and turned into an error event by the caller.
    private static async Task<(bool HasNext, Exception? Error)> TryMoveNextAsync(
        IAsyncEnumerator<ResponseStreamChunk> stream
    )
    {
        try
        {
            return (await stream.MoveNextAsync(), null);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return (false, ex);
        }
    }
}

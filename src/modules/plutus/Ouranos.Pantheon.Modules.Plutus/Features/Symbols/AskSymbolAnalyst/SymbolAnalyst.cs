using System.Diagnostics;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetDailySymbolSummary.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst;

public sealed class SymbolAnalyst(
    ILogger<SymbolAnalyst> logger,
    IOuranosMachineLearningClient mlClient,
    IMessageBus bus,
    IMemoryCache cache,
    IOptions<PlutusOptions> options,
    IEnumerable<IAssistantTool<SymbolAnalystContext>> tools
) : PantheonAssistant<SymbolAnalystContext>(logger, mlClient, tools)
{
    private readonly ILogger<SymbolAnalyst> _logger = Guard.Against.Null(logger);
    private readonly IMessageBus _bus = Guard.Against.Null(bus);
    private readonly IMemoryCache _cache = Guard.Against.Null(cache);
    private readonly IOptions<PlutusOptions> _options = Guard.Against.Null(options);

    protected override async ValueTask<AssistantPrompt> BuildPromptAsync(
        SymbolAnalystContext context,
        CancellationToken cancellationToken
    )
    {
        _logger.LogTrace(
            "Attempting to build symbol analyst prompt for symbol '{symbolId}'.",
            context.SymbolId
        );
        cancellationToken.ThrowIfCancellationRequested();

        var options = _options.Value.SymbolAnalyst;
        var systemPrompt = await ComposeSystemPromptAsync(context, options, cancellationToken);

        _logger.LogDebug(
            "Successfully built symbol analyst prompt for symbol '{symbolId}'.",
            context.SymbolId
        );
        return new AssistantPrompt(
            options.ModelName,
            systemPrompt,
            options.Temperature,
            options.MaxTokens,
            options.ReasoningEffort
        );
    }

    // Follow-up turns reuse the same system prompt so the inference server can restore its cached
    // prompt state instead of re-processing the whole context, which dominates latency on CPU.
    private async Task<string> ComposeSystemPromptAsync(
        SymbolAnalystContext context,
        SymbolAnalystOptions options,
        CancellationToken cancellationToken
    )
    {
        var cacheKey = $"{nameof(SymbolAnalyst)}:{context.SymbolId}:{context.TimeFrame}";

        if (_cache.TryGetValue(cacheKey, out string? cached) && cached is not null)
        {
            return cached;
        }

        var snapshot = await LoadSnapshotAsync(context, cancellationToken);
        var systemPrompt = SymbolAnalystPrompt.Compose(snapshot);

        _cache.Set(cacheKey, systemPrompt, TimeSpan.FromMinutes(options.ContextCacheMinutes));
        return systemPrompt;
    }

    private async Task<SymbolAnalystSnapshot> LoadSnapshotAsync(
        SymbolAnalystContext context,
        CancellationToken cancellationToken
    )
    {
        var startedAt = Stopwatch.GetTimestamp();
        var generatedAt = DateTimeOffset.UtcNow;
        var symbolId = context.SymbolId;

        var today = _bus.InvokeAsync<GetDailySymbolSummaryResponse>(
            new GetDailySymbolSummaryInput(symbolId),
            cancellationToken
        );

        var (symbol, market) = await SymbolAnalystQueries.LoadSymbolAsync(
            _bus,
            symbolId,
            cancellationToken
        );

        var snapshot = new SymbolAnalystSnapshot(
            symbol,
            market,
            context.TimeFrame,
            await today,
            generatedAt
        );

        _logger.LogDebug(
            "Loaded symbol analyst context for symbol '{symbolId}' in {elapsedMs} ms.",
            symbolId,
            Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds
        );
        return snapshot;
    }
}

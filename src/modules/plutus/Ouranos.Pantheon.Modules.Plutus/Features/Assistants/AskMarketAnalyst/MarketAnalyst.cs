using System.Diagnostics;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.AskMarketAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketOverview.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.AskMarketAnalyst;

public sealed class MarketAnalyst(
    ILogger<MarketAnalyst> logger,
    IOuranosMachineLearningClient mlClient,
    IMessageBus bus,
    IMemoryCache cache,
    IOptions<PlutusOptions> options,
    IEnumerable<IAssistantTool<MarketAnalystContext>> tools
) : PantheonAssistant<MarketAnalystContext>(logger, mlClient, tools)
{
    private readonly ILogger<MarketAnalyst> _logger = Guard.Against.Null(logger);
    private readonly IMessageBus _bus = Guard.Against.Null(bus);
    private readonly IMemoryCache _cache = Guard.Against.Null(cache);
    private readonly IOptions<PlutusOptions> _options = Guard.Against.Null(options);

    protected override async ValueTask<AssistantPrompt> BuildPromptAsync(
        MarketAnalystContext context,
        CancellationToken cancellationToken
    )
    {
        _logger.LogTrace(
            "Attempting to build market analyst prompt for market '{marketId}'.",
            context.MarketId
        );
        cancellationToken.ThrowIfCancellationRequested();

        var options = _options.Value.MarketAnalyst;
        var systemPrompt = await ComposeSystemPromptAsync(context, options, cancellationToken);

        _logger.LogDebug(
            "Successfully built market analyst prompt for market '{marketId}'.",
            context.MarketId
        );
        return new AssistantPrompt(
            options.ModelName,
            systemPrompt,
            options.Temperature,
            options.MaxTokens,
            options.ReasoningEffort
        );
    }

    private async Task<string> ComposeSystemPromptAsync(
        MarketAnalystContext context,
        MarketAnalystOptions options,
        CancellationToken cancellationToken
    )
    {
        var cacheKey = CreateCacheKey(context);

        if (_cache.TryGetValue(cacheKey, out string? cached) && cached is not null)
        {
            return cached;
        }

        var snapshot = await LoadSnapshotAsync(context, options, cancellationToken);
        var systemPrompt = MarketAnalystPrompt.Compose(snapshot);

        _cache.Set(cacheKey, systemPrompt, TimeSpan.FromMinutes(options.ContextCacheMinutes));
        return systemPrompt;
    }

    private async Task<MarketAnalystSnapshot> LoadSnapshotAsync(
        MarketAnalystContext context,
        MarketAnalystOptions options,
        CancellationToken cancellationToken
    )
    {
        var startedAt = Stopwatch.GetTimestamp();
        var generatedAt = DateTimeOffset.UtcNow;

        var market = await _bus.InvokeAsync<GetMarketResponse>(
            new GetMarketInput(context.MarketId),
            cancellationToken
        );

        var overview = _bus.InvokeAsync<GetMarketOverviewResponse>(
            new GetMarketOverviewInput(market.Id, context.TimeFrame),
            cancellationToken
        );

        var view = LoadViewAsync(market.Id, context, options, cancellationToken);

        var snapshot = new MarketAnalystSnapshot(
            market,
            context,
            await overview,
            await view,
            options.TrendPoints,
            generatedAt
        );

        _logger.LogDebug(
            "Loaded market analyst context for market '{marketId}' in {elapsedMs} ms.",
            market.Id,
            Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds
        );
        return snapshot;
    }

    private async Task<PagedResponse<GetMarketTradesResponse>?> LoadViewAsync(
        Id<Market> marketId,
        MarketAnalystContext context,
        MarketAnalystOptions options,
        CancellationToken cancellationToken
    )
    {
        if (context.View is not { } view)
        {
            return null;
        }

        return await _bus.InvokeAsync<PagedResponse<GetMarketTradesResponse>>(
            new GetMarketTradesInput(
                marketId,
                context.TimeFrame,
                view.SortField,
                view.SortDirection,
                view.Skip,
                options.ClampViewRows(view.Take),
                view.Filter
            ),
            cancellationToken
        );
    }

    private static string CreateCacheKey(MarketAnalystContext context)
    {
        var key = $"{nameof(MarketAnalyst)}:{context.MarketId}:{context.TimeFrame}";

        if (context.View is not { } view)
        {
            return key;
        }

        var filter = string.Join('|', view.Filter ?? []);
        return $"{key}:{view.SortField}:{view.SortDirection}:{view.Skip}:{view.Take}:{filter}";
    }
}

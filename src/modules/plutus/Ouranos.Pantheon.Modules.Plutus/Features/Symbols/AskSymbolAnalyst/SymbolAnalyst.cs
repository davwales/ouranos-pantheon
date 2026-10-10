using System.Diagnostics;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetForecastEfficacy.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.GetAllPositions.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignalHistory.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetAllTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetSymbolTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst;

public sealed class SymbolAnalyst(
    ILogger<SymbolAnalyst> logger,
    IOuranosMachineLearningClient mlClient,
    IMessageBus bus,
    IMemoryCache cache,
    IOptions<PlutusOptions> options
) : PantheonAssistant<SymbolAnalystContext>(logger, mlClient)
{
    internal static readonly TimeFrame[] Windows =
    [
        TimeFrame.OneDay,
        TimeFrame.OneWeek,
        TimeFrame.OneMonth,
    ];

    internal static readonly TimeSpan ForecastEfficacyWindow = TimeSpan.FromDays(30);

    private const int MaxForecastEfficacyRows = 50;

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

        var snapshot = await LoadSnapshotAsync(context, options, cancellationToken);
        var systemPrompt = SymbolAnalystPrompt.Compose(snapshot);

        _cache.Set(cacheKey, systemPrompt, TimeSpan.FromMinutes(options.ContextCacheMinutes));
        return systemPrompt;
    }

    private async Task<SymbolAnalystSnapshot> LoadSnapshotAsync(
        SymbolAnalystContext context,
        SymbolAnalystOptions options,
        CancellationToken cancellationToken
    )
    {
        var startedAt = Stopwatch.GetTimestamp();
        var generatedAt = DateTimeOffset.UtcNow;
        var symbolId = context.SymbolId;
        string[] symbolFilter = [$"SymbolId:eq:{symbolId}"];

        var symbol = await _bus.InvokeAsync<GetSymbolResponse>(
            new GetSymbolInput(symbolId),
            cancellationToken
        );

        var market = await _bus.InvokeAsync<GetMarketResponse>(
            new GetMarketInput(symbol.MarketId),
            cancellationToken
        );

        var windows = LoadWindowsAsync(market.Id, symbolFilter, cancellationToken);
        var latestTrade = LoadLatestTradeAsync(symbolFilter, cancellationToken);
        var forecast = LoadForecastAsync(market, symbolFilter, cancellationToken);
        var positions = LoadPositionsAsync(market.Id, symbolFilter, options, cancellationToken);

        var selectedTrades = _bus.InvokeAsync<GetSymbolTradesResponse>(
            new GetSymbolTradesInput(symbolId, context.TimeFrame, options.ChartBuckets),
            cancellationToken
        );

        var signals = _bus.InvokeAsync<GetSymbolSignalsResponse>(
            new GetSymbolSignalsInput(symbolId),
            cancellationToken
        );

        var signalHistory = _bus.InvokeAsync<GetSymbolSignalHistoryResponse>(
            new GetSymbolSignalHistoryInput(symbolId),
            cancellationToken
        );

        var forecastEfficacy = LoadForecastEfficacyAsync(
            market,
            symbolId,
            generatedAt,
            cancellationToken
        );

        var snapshot = new SymbolAnalystSnapshot(
            symbol,
            market,
            context.TimeFrame,
            await selectedTrades,
            await windows,
            await latestTrade,
            await signals,
            await signalHistory,
            await forecast,
            await forecastEfficacy,
            await positions,
            generatedAt
        );

        _logger.LogDebug(
            "Loaded symbol analyst context for symbol '{symbolId}' in {elapsedMs} ms.",
            symbolId,
            Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds
        );
        return snapshot;
    }

    private Task<SymbolAnalystWindow[]> LoadWindowsAsync(
        Id<Market> marketId,
        string[] symbolFilter,
        CancellationToken cancellationToken
    )
    {
        return Task.WhenAll(
            Windows.Select(timeFrame =>
                LoadWindowAsync(marketId, timeFrame, symbolFilter, cancellationToken)
            )
        );
    }

    private async Task<SymbolAnalystWindow> LoadWindowAsync(
        Id<Market> marketId,
        TimeFrame timeFrame,
        string[] symbolFilter,
        CancellationToken cancellationToken
    )
    {
        var trades = await _bus.InvokeAsync<PagedResponse<GetMarketTradesResponse>>(
            new GetMarketTradesInput(marketId, timeFrame, Take: 1, Filter: symbolFilter),
            cancellationToken
        );

        return new SymbolAnalystWindow(timeFrame, trades.Items.FirstOrDefault());
    }

    private async Task<GetAllTradesResponse?> LoadLatestTradeAsync(
        string[] symbolFilter,
        CancellationToken cancellationToken
    )
    {
        var trades = await _bus.InvokeAsync<List<GetAllTradesResponse>>(
            new GetAllTradesInput(
                TimeFrame.OneYear,
                nameof(GetAllTradesResponse.Timestamp),
                "desc",
                Take: 1,
                Filter: symbolFilter
            ),
            cancellationToken
        );

        return trades.FirstOrDefault();
    }

    private async Task<GetMarketForecastResponse?> LoadForecastAsync(
        GetMarketResponse market,
        string[] symbolFilter,
        CancellationToken cancellationToken
    )
    {
        if (!market.IsForecastingEnabled)
        {
            return null;
        }

        var forecasts = await _bus.InvokeAsync<PagedResponse<GetMarketForecastResponse>>(
            new GetMarketForecastInput(market.Id, Take: 1, Filter: symbolFilter),
            cancellationToken
        );

        return forecasts.Items.FirstOrDefault();
    }

    private async Task<IReadOnlyList<GetForecastEfficacyResponse>> LoadForecastEfficacyAsync(
        GetMarketResponse market,
        Id<Symbol> symbolId,
        DateTimeOffset generatedAt,
        CancellationToken cancellationToken
    )
    {
        if (!market.IsForecastingEnabled)
        {
            return [];
        }

        var efficacy = await _bus.InvokeAsync<PagedResponse<GetForecastEfficacyResponse>>(
            new GetForecastEfficacyInput(
                SymbolId: symbolId.Value,
                Since: generatedAt - ForecastEfficacyWindow,
                Take: MaxForecastEfficacyRows
            ),
            cancellationToken
        );

        return [.. efficacy.Items];
    }

    private async Task<IReadOnlyList<GetAllPositionsResponse>> LoadPositionsAsync(
        Id<Market> marketId,
        string[] symbolFilter,
        SymbolAnalystOptions options,
        CancellationToken cancellationToken
    )
    {
        var positions = await _bus.InvokeAsync<PagedResponse<GetAllPositionsResponse>>(
            new GetAllPositionsInput(
                marketId,
                SortField: nameof(GetAllPositionsResponse.CreatedAt),
                SortDirection: "desc",
                Take: options.MaxPositions,
                Filter: symbolFilter
            ),
            cancellationToken
        );

        return [.. positions.Items];
    }
}

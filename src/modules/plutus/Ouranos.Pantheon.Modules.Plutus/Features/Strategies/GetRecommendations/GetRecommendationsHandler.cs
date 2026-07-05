using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetRecommendations.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Executors;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Application;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetRecommendations;

public sealed class GetRecommendationsHandler
    : IPantheonHandler<GetRecommendationsInput, GetRecommendationsResponse>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetRecommendationsHandler> _logger;
    private readonly Dictionary<StrategyType, IStrategyExecutor> _executors;

    public GetRecommendationsHandler(
        ILogger<GetRecommendationsHandler> logger,
        PlutusDbContext dbContext,
        IEnumerable<IStrategyExecutor> executors,
        CompositeExecutor compositeExecutor
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);
        Guard.Against.Null(compositeExecutor);

        _logger = logger;
        _dbContext = dbContext;
        _executors = executors.ToDictionary(e => e.SupportedType);
        _executors[StrategyType.Composite] = compositeExecutor;
    }

    public async Task<GetRecommendationsResponse> Handle(
        GetRecommendationsInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get recommendations query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        Guard.Against.NegativeOrZero(query.Budget, nameof(query.Budget));

        var strategy = await _dbContext.Strategies.FirstOrDefaultAsync(
            s => s.Id == query.StrategyId,
            cancellationToken
        );
        Guard.Against.NotFound(query.StrategyId, strategy);

        var market = await _dbContext.Markets.FirstOrDefaultAsync(
            m => m.Id == query.MarketId,
            cancellationToken
        );
        Guard.Against.NotFound(query.MarketId, market);

        Guard.Against.InvalidInput(
            query.MarketId,
            nameof(query.MarketId),
            m => m == strategy.MarketId,
            $"Strategy '{strategy.Id}' does not belong to market '{query.MarketId}'."
        );

        var symbols = await _dbContext
            .Symbols.AsNoTracking()
            .Where(s => s.MarketId == query.MarketId)
            .ToListAsync(cancellationToken);

        if (symbols.Count == 0)
        {
            return new GetRecommendationsResponse([]);
        }

        var symbolIds = symbols.Select(s => s.Id).ToList();
        var snapshots = await _dbContext
            .MarketTradeSnapshots.AsNoTracking()
            .Where(s => symbolIds.Contains(s.SymbolId) && s.MarketId == query.MarketId)
            .ToListAsync(cancellationToken);

        var latestRows = await _dbContext
            .LatestSignals.AsNoTracking()
            .Where(s => symbolIds.Contains(s.SymbolId))
            .ToListAsync(cancellationToken);

        var signals = latestRows
            .Select(ls => Signal.Create(query.MarketId, ls.SymbolId, ls.SignalType, ls.LastValue))
            .ToList();

        var forecasts = await _dbContext
            .Forecasts.AsNoTracking()
            .Include(f => f.Predictions)
            .Where(f => symbolIds.Contains(f.SymbolId) && f.MarketId == query.MarketId)
            .ToListAsync(cancellationToken);

        if (!_executors.TryGetValue(strategy.Type, out var executor))
        {
            throw new InvalidOperationException(
                $"No executor registered for strategy type '{strategy.Type}'."
            );
        }

        var taxRate = market.Taxes.Flat?.Rate ?? 0m;
        var buyThreshold = strategy.SignalWeightedConfig?.BuyThreshold ?? 0.1m;
        var maxPositions = strategy.TradingConfiguration.MaxPositions ?? int.MaxValue;
        var maxPositionPercent = strategy.TradingConfiguration.MaxPositionPercent ?? 1m;

        var recommendations = new List<StrategyRecommendation>();

        foreach (var symbol in symbols)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var symbolShort = snapshots.FirstOrDefault(s =>
                s.SymbolId == symbol.Id && s.TimeFrame == TimeFrame.OneHour
            );
            var symbolMedium = snapshots.FirstOrDefault(s =>
                s.SymbolId == symbol.Id && s.TimeFrame == TimeFrame.OneWeek
            );
            var symbolLong = snapshots.FirstOrDefault(s =>
                s.SymbolId == symbol.Id && s.TimeFrame == TimeFrame.OneMonth
            );
            var currentPrice =
                symbolShort?.TotalSpent > 0 && symbolShort.TotalVolume > 0
                    ? symbolShort.TotalSpent / symbolShort.TotalVolume
                    : 0m;

            if (currentPrice == 0 || symbolShort is null)
            {
                continue;
            }

            var symbolSignals = signals.Where(s => s.SymbolId == symbol.Id).ToList();
            var forecast = forecasts.FirstOrDefault(f => f.SymbolId == symbol.Id);
            var (forecastedPrice, forecastedChange) = GetForecastData(forecast, currentPrice);

            var limit = market.Taxes.Flat?.Maximum ?? decimal.MaxValue;

            var context = new StrategyScoreContext(
                symbol.Id,
                query.MarketId,
                symbol.Name,
                symbol.Subcode,
                currentPrice,
                taxRate,
                limit,
                symbolShort,
                symbolMedium,
                symbolLong,
                [],
                symbolSignals,
                forecastedPrice,
                forecastedChange,
                SignalWeightedConfig: strategy.SignalWeightedConfig,
                ForecastMomentumConfig: strategy.ForecastMomentumConfig,
                MeanReversionConfig: strategy.MeanReversionConfig,
                RecipeArbitrageConfig: strategy.RecipeArbitrageConfig,
                Components: strategy.Components
            );

            var score = executor.Score(context, strategy.TradingConfiguration);
            if (score is null || score.Value <= buyThreshold)
            {
                continue;
            }

            var positionBudget = query.Budget * maxPositionPercent;
            var volume = Math.Max(1, Math.Floor(positionBudget / currentPrice));
            var allocation = volume * currentPrice;

            recommendations.Add(
                new StrategyRecommendation(
                    symbol.Id.ToString(),
                    symbol.Name,
                    symbol.Subcode,
                    score.Value,
                    allocation,
                    currentPrice,
                    volume,
                    BuildRationale(score.Value, symbolSignals, symbolMedium, context)
                )
            );
        }

        var sorted = recommendations.OrderByDescending(r => r.Score).Take(maxPositions).ToList();

        _logger.LogDebug(
            "Successfully handled get recommendations request. {count} recommendations.",
            sorted.Count
        );
        return new GetRecommendationsResponse(sorted);
    }

    private static (decimal? Price, decimal? Change) GetForecastData(
        Forecast? forecast,
        decimal currentPrice
    )
    {
        if (forecast is null || currentPrice == 0)
        {
            return (null, null);
        }

        var forecastedPrice = forecast.Latest.AveragePrice;
        if (forecastedPrice == 0)
        {
            return (forecastedPrice, null);
        }

        return (forecastedPrice, (forecastedPrice - currentPrice) / currentPrice);
    }

    private static string BuildRationale(
        decimal score,
        List<Signal> signals,
        MarketTradeSnapshot? snap,
        StrategyScoreContext context
    )
    {
        var parts = new List<string> { $"Score: {score:F3}" };

        if (signals.Count > 0)
        {
            var topSignals = signals.OrderByDescending(s => Math.Abs(s.Value)).Take(3);
            parts.Add(
                $"Top signals: {string.Join(", ", topSignals.Select(s => $"{s.Type}={s.Value:F2}"))}"
            );
        }

        if (snap is not null)
        {
            parts.Add($"Price range: {snap.MinPrice:F2}-{snap.MaxPrice:F2}");
        }

        if (context.ForecastedPriceChange.HasValue)
        {
            parts.Add($"Forecast change: {context.ForecastedPriceChange.Value:P1}");
        }

        return string.Join("; ", parts);
    }
}

using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application.Pipeline;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Executors;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Steps;

public sealed class InitializeStep : IStep<BacktestPayload>
{
    private readonly ILogger<InitializeStep> _logger;
    private readonly IBacktestDataQueryService _dataService;
    private readonly Dictionary<StrategyType, IStrategyExecutor> _executors;

    public InitializeStep(
        ILogger<InitializeStep> logger,
        IBacktestDataQueryService dataService,
        IEnumerable<IStrategyExecutor> executors,
        CompositeExecutor compositeExecutor
    )
    {
        _logger = Guard.Against.Null(logger);
        _dataService = Guard.Against.Null(dataService);

        _executors = new Dictionary<StrategyType, IStrategyExecutor> { [StrategyType.Composite] = compositeExecutor };

        foreach (var executor in executors)
        {
            _executors.TryAdd(executor.SupportedType, executor);
        }
    }

    public async Task ExecuteAsync(PipelineContext context, BacktestPayload payload)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        var data = payload.Data ?? await _dataService.LoadDataAsync(
            payload.Parameters.MarketId,
            payload.Parameters.StartDate,
            payload.Parameters.EndDate,
            context.CancellationToken,
            lookbackDays: 30
        );

        var taxRate = GetTaxRate(data.Market);
        var executor = ResolveExecutor(payload.Parameters.Strategy.Type);
        var windowDays = DetermineWindowSize(payload.Parameters.TotalDays);

        payload.Context = new BacktestContext(
            data,
            executor,
            taxRate,
            windowDays,
            payload.Parameters.StartDate
        );

        _logger.LogDebug(
            "Running backtest for strategy '{strategyId}' with {symbolCount} symbols, {totalDays} days, {windowDays}-day windows.",
            payload.Parameters.Strategy.Id,
            payload.Context.Data.Symbols.Count,
            payload.Parameters.TotalDays,
            windowDays
        );
    }

    private IStrategyExecutor ResolveExecutor(StrategyType type)
    {
        if (!_executors.TryGetValue(type, out var executor))
        {
            throw new InvalidOperationException($"No executor registered for strategy type '{type}'.");
        }

        return executor;
    }

    public static decimal GetTaxRate(Market market)
    {
        return market.Taxes.Flat?.Rate ?? 0m;
    }

    public static int DetermineWindowSize(int totalDays)
    {
        if (totalDays <= 30)
        {
            return 1;
        }

        if (totalDays <= 90)
        {
            return 3;
        }

        if (totalDays <= 365)
        {
            return 7;
        }

        return 14;
    }
}

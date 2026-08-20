using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;
using Ouranos.Pantheon.Modules.Shared.Application.Pipeline;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Steps;

public sealed class InitializeStep(
    ILogger<InitializeStep> logger,
    IBacktestDataQueryService dataService,
    IStrategyExecutor executor
) : IStep<BacktestPayload>
{
    private readonly ILogger<InitializeStep> _logger = Guard.Against.Null(logger);
    private readonly IBacktestDataQueryService _dataService = Guard.Against.Null(dataService);
    private readonly IStrategyExecutor _executor = Guard.Against.Null(executor);

    public async Task ExecuteAsync(PipelineContext context, BacktestPayload payload)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        var data =
            payload.Data
            ?? await _dataService.LoadDataAsync(
                payload.Parameters.MarketId,
                payload.Parameters.StartDate,
                payload.Parameters.EndDate,
                context.CancellationToken,
                lookbackDays: 30
            );

        var taxRate = GetTaxRate(data.Market);

        payload.Context = new BacktestContext(
            data,
            _executor,
            taxRate,
            payload.Parameters.StartDate,
            payload.Parameters.InputWeights,
            payload.Parameters.Thresholds
        );

        _logger.LogDebug(
            "Running backtest for strategy '{strategyId}' with {symbolCount} symbols, {totalDays} days.",
            payload.Parameters.Strategy.Id,
            payload.Context.Data.Symbols.Count,
            payload.Parameters.TotalDays
        );
    }

    public static decimal GetTaxRate(Market market)
    {
        return market.Taxes.Flat?.Rate ?? 0m;
    }
}

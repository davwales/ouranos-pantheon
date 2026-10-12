using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Engine;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Events;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Wolverine.Attributes;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.OptimizeStrategy;

public sealed class OptimizeStrategyConsumer(
    ILogger<OptimizeStrategyConsumer> logger,
    BacktestJobRunner runner,
    StrategyEngines engines,
    StrategyOptimizer optimizer
) : IPantheonHandler<OptimizeStrategyMessage>
{
    private const int DataLoadedProgress = 2;

    private readonly ILogger<OptimizeStrategyConsumer> _logger = Guard.Against.Null(logger);
    private readonly BacktestJobRunner _runner = Guard.Against.Null(runner);
    private readonly StrategyEngines _engines = Guard.Against.Null(engines);
    private readonly StrategyOptimizer _optimizer = Guard.Against.Null(optimizer);

    [MessageTimeout(3600)]
    public async Task Handle(
        OptimizeStrategyMessage message,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace(
            "Processing optimization for backtest '{backtestId}'.",
            message.BacktestId
        );
        cancellationToken.ThrowIfCancellationRequested();

        await _runner.RunAsync(
            message.BacktestId,
            async (backtest, monitor) =>
            {
                var engine = _engines.For(backtest.Strategy.Configuration);
                var data = await engine.LoadAsync(
                    backtest.MarketId,
                    backtest.StartDate,
                    backtest.EndDate,
                    backtest.Strategy.Configuration,
                    monitor.CancellationToken
                );
                monitor.Report(DataLoadedProgress, "Market data loaded, starting optimization...");

                return await _optimizer.OptimizeAsync(engine, data, backtest, monitor);
            },
            cancellationToken
        );

        _logger.LogDebug("Processed optimization for backtest '{backtestId}'.", message.BacktestId);
    }
}

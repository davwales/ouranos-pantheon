using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Engine;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Events;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Wolverine.Attributes;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;

public sealed class RunBacktestConsumer(
    ILogger<RunBacktestConsumer> logger,
    BacktestJobRunner runner,
    StrategyEngines engines
) : IPantheonHandler<RunBacktestMessage>
{
    private const int SimulationProgressStart = 5;
    private const int SimulationProgressEnd = 95;

    private readonly ILogger<RunBacktestConsumer> _logger = Guard.Against.Null(logger);
    private readonly BacktestJobRunner _runner = Guard.Against.Null(runner);
    private readonly StrategyEngines _engines = Guard.Against.Null(engines);

    [MessageTimeout(3600)]
    public async Task Handle(
        RunBacktestMessage message,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Processing backtest '{backtestId}'.", message.BacktestId);
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

                var settings = new SimulationSettings(
                    backtest.StartDate,
                    backtest.EndDate,
                    backtest.Budget,
                    backtest.RunSettings.VolumeParticipationRate,
                    backtest.RunSettings.SlippageMultiplier,
                    monitor.Phase(SimulationProgressStart, SimulationProgressEnd, "Simulating..."),
                    monitor.CancellationToken
                );

                var result = engine.Simulate(data, backtest.Strategy.Configuration, settings);
                return new BacktestOutcome(result.ToResults(settings), result.Positions);
            },
            cancellationToken
        );

        _logger.LogDebug("Processed backtest '{backtestId}'.", message.BacktestId);
    }
}

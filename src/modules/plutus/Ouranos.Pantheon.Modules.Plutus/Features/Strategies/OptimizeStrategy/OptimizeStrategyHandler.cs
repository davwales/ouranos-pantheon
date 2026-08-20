using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.OptimizeStrategy.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Events;
using Ouranos.Pantheon.Modules.Shared.Application;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.OptimizeStrategy;

public sealed class OptimizeStrategyHandler
    : IPantheonHandler<OptimizeStrategyInput, OptimizeStrategyResponse>
{
    private readonly IDbContextFactory<PlutusDbContext> _dbContextFactory;
    private readonly ILogger<OptimizeStrategyHandler> _logger;
    private readonly IMessageBus _bus;

    public OptimizeStrategyHandler(
        ILogger<OptimizeStrategyHandler> logger,
        IDbContextFactory<PlutusDbContext> dbContextFactory,
        IMessageBus bus
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContextFactory);
        Guard.Against.Null(bus);

        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _bus = bus;
    }

    public async Task<OptimizeStrategyResponse> Handle(
        OptimizeStrategyInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle optimize strategy command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        Guard.Against.OutOfRange(command.Generations, nameof(command.Generations), 1, 500);
        Guard.Against.OutOfRange(command.PopulationSize, nameof(command.PopulationSize), 2, 200);
        Guard.Against.NegativeOrZero(command.Budget, nameof(command.Budget));
        Guard.Against.InvalidInput(
            command.EndDate,
            nameof(command.EndDate),
            d => d > command.StartDate,
            "End date must be after start date."
        );
        Guard.Against.InvalidInput(
            command.OutSampleRatio,
            nameof(command.OutSampleRatio),
            r => r is > 0 and < 1,
            "Out-of-sample ratio must be between 0 and 1 (exclusive)."
        );

        var strategy = await dbContext.Strategies.FirstOrDefaultAsync(
            s => s.Id == command.StrategyId,
            cancellationToken
        );

        Guard.Against.NotFound(command.StrategyId, strategy);

        var backtest = Backtest.Create(
            command.StrategyId,
            command.MarketId,
            command.StartDate,
            command.EndDate,
            command.Budget,
            strategy,
            BacktestKind.Optimization
        );

        await dbContext.Backtests.AddAsync(backtest, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await _bus.PublishAsync(
            new OptimizeStrategyMessage(
                backtest.Id,
                (uint)command.Generations,
                (uint)command.PopulationSize,
                command.SortinoWeight,
                command.CagrWeight,
                command.DrawdownWeight,
                command.TurnoverWeight,
                command.L1RegularizationWeight,
                command.OutSampleRatio,
                command.VolumeParticipationRate ?? 0.25m,
                command.SlippageMultiplier ?? 0.1m,
                command.MinTrades
            )
        );

        _logger.LogDebug(
            "Successfully handled optimize strategy command. Backtest ID: '{backtestId}'.",
            backtest.Id
        );
        return new OptimizeStrategyResponse(backtest.Id);
    }
}

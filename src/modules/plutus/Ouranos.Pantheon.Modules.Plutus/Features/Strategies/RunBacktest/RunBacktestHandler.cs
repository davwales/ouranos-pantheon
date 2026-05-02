using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Events;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;

public sealed class RunBacktestHandler : IPantheonHandler<RunBacktestInput, RunBacktestResponse>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<RunBacktestHandler> _logger;
    private readonly IMessageBus _bus;

    public RunBacktestHandler(
        ILogger<RunBacktestHandler> logger,
        PlutusDbContext dbContext,
        IMessageBus bus
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);
        Guard.Against.Null(bus);

        _logger = logger;
        _dbContext = dbContext;
        _bus = bus;
    }

    public async Task<RunBacktestResponse> Handle(
        RunBacktestInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle run backtest command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var strategy = await _dbContext.Strategies
            .FirstOrDefaultAsync(s => s.Id == command.StrategyId, cancellationToken);

        Guard.Against.NotFound(command.StrategyId, strategy);

        var backtest = Backtest.Create(
            command.StrategyId,
            command.MarketId,
            command.StartDate,
            command.EndDate,
            command.Budget,
            strategy
        );

        await _dbContext.Backtests.AddAsync(backtest, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _bus.PublishAsync(new RunBacktestMessage(backtest.Id));

        _logger.LogDebug("Successfully handled run backtest command. Backtest ID: '{backtestId}'.", backtest.Id);
        return new RunBacktestResponse(backtest.Id);
    }
}
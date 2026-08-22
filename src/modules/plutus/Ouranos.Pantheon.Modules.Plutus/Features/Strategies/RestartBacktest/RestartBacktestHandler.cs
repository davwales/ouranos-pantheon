using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RestartBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Events;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RestartBacktest;

public sealed class RestartBacktestHandler
    : IPantheonHandler<RestartBacktestInput, RestartBacktestResponse>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<RestartBacktestHandler> _logger;
    private readonly IMessageBus _bus;

    public RestartBacktestHandler(
        ILogger<RestartBacktestHandler> logger,
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

    public async Task<RestartBacktestResponse> Handle(
        RestartBacktestInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle restart backtest command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var backtest = await _dbContext.Backtests.FirstOrDefaultAsync(
            b => b.Id == command.BacktestId,
            cancellationToken
        );

        Guard.Against.NotFound(command.BacktestId, backtest);

        backtest.Restart();
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _bus.PublishAsync(new RunBacktestMessage(backtest.Id));

        _logger.LogDebug("Successfully restarted backtest '{backtestId}'.", backtest.Id);
        return new RestartBacktestResponse(backtest.Id, backtest.Status);
    }
}

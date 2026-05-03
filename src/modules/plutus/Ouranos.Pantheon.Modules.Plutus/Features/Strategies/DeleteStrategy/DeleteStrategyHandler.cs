using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.DeleteStrategy.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.DeleteStrategy;

public sealed class DeleteStrategyHandler : IPantheonHandler<DeleteStrategyInput, IdResponse<Strategy>>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<DeleteStrategyHandler> _logger;

    public DeleteStrategyHandler(
        ILogger<DeleteStrategyHandler> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<IdResponse<Strategy>> Handle(
        DeleteStrategyInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle delete strategy command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var strategy = await _dbContext.Strategies
            .FirstOrDefaultAsync(s => s.Id == command.StrategyId, cancellationToken);

        Guard.Against.NotFound(command.StrategyId, strategy);

        _dbContext.Strategies.Remove(strategy);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully handled delete strategy command.");
        return new IdResponse<Strategy>(command.StrategyId);
    }
}
using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.SetStrategyActive.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.SetStrategyActive;

public sealed class SetStrategyActiveHandler : IPantheonHandler<SetStrategyActiveInput, IdResponse<Strategy>>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<SetStrategyActiveHandler> _logger;

    public SetStrategyActiveHandler(
        ILogger<SetStrategyActiveHandler> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<IdResponse<Strategy>> Handle(
        SetStrategyActiveInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle set strategy active command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var strategy = await _dbContext.Strategies
            .FirstOrDefaultAsync(s => s.Id == command.StrategyId, cancellationToken);

        Guard.Against.NotFound(command.StrategyId, strategy);

        strategy.SetActive(command.IsActive);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully handled set strategy active command.");
        return new IdResponse<Strategy>(strategy.Id);
    }
}
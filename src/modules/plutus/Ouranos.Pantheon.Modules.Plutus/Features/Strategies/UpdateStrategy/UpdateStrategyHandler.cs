using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.UpdateStrategy.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Application.Common;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.UpdateStrategy;

public sealed class UpdateStrategyHandler
    : IPantheonHandler<UpdateStrategyInput, IdResponse<Strategy>>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<UpdateStrategyHandler> _logger;

    public UpdateStrategyHandler(ILogger<UpdateStrategyHandler> logger, PlutusDbContext dbContext)
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<IdResponse<Strategy>> Handle(
        UpdateStrategyInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle update strategy command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var strategy = await _dbContext.Strategies.FirstOrDefaultAsync(
            s => s.Id == command.StrategyId,
            cancellationToken
        );

        Guard.Against.NotFound(command.StrategyId, strategy);

        strategy.Update(
            command.Name,
            command.Description,
            command.Configuration,
            command.InputWeights,
            command.Thresholds
        );

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully handled update strategy command.");
        return new IdResponse<Strategy>(strategy.Id);
    }
}

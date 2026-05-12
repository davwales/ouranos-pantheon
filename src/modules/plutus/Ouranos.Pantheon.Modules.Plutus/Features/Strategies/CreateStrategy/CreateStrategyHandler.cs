using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.CreateStrategy.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Application.Common;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.CreateStrategy;

public sealed class CreateStrategyHandler
    : IPantheonHandler<CreateStrategyInput, IdResponse<Strategy>>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<CreateStrategyHandler> _logger;

    public CreateStrategyHandler(ILogger<CreateStrategyHandler> logger, PlutusDbContext dbContext)
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<IdResponse<Strategy>> Handle(
        CreateStrategyInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle create strategy command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var strategy = Strategy.Create(
            command.MarketId,
            command.Name,
            command.Description,
            command.Type,
            command.Configuration,
            command.SignalWeightedConfig,
            command.ForecastMomentumConfig,
            command.MeanReversionConfig,
            command.RecipeArbitrageConfig
        );

        await _dbContext.Strategies.AddAsync(strategy, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully handled create strategy command.");
        return new IdResponse<Strategy>(strategy.Id);
    }
}

using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetStrategy.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetStrategy;

public sealed class GetStrategyHandler : IPantheonHandler<GetStrategyInput, GetStrategyResponse>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetStrategyHandler> _logger;

    public GetStrategyHandler(
        ILogger<GetStrategyHandler> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<GetStrategyResponse> Handle(
        GetStrategyInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get strategy query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var strategy = await _dbContext.Strategies
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == query.StrategyId, cancellationToken);

        Guard.Against.NotFound(query.StrategyId, strategy);

        _logger.LogDebug("Successfully handled get strategy request.");
        return new GetStrategyResponse(
            strategy.Id,
            strategy.MarketId,
            strategy.Name,
            strategy.Description,
            strategy.Type,
            strategy.TradingConfiguration,
            strategy.SignalWeightedConfig,
            strategy.ForecastMomentumConfig,
            strategy.MeanReversionConfig,
            strategy.RecipeArbitrageConfig,
            strategy.IsActive,
            strategy.CreatedAt,
            strategy.UpdatedAt
        );
    }
}
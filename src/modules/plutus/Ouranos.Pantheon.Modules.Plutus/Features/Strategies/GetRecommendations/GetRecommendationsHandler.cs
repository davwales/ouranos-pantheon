using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetRecommendations.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Engine;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetRecommendations;

public sealed class GetRecommendationsHandler(
    ILogger<GetRecommendationsHandler> logger,
    PlutusDbContext dbContext,
    StrategyEngines engines
) : IPantheonHandler<GetRecommendationsInput, GetRecommendationsResponse>
{
    private readonly ILogger<GetRecommendationsHandler> _logger = Guard.Against.Null(logger);
    private readonly PlutusDbContext _dbContext = Guard.Against.Null(dbContext);
    private readonly StrategyEngines _engines = Guard.Against.Null(engines);

    public async Task<GetRecommendationsResponse> Handle(
        GetRecommendationsInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get recommendations query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        Guard.Against.NegativeOrZero(query.Budget, nameof(query.Budget));
        Guard.Against.InvalidInput(
            query.VolumeParticipationRate,
            nameof(query.VolumeParticipationRate),
            r => r is null or > 0m and <= 1m,
            "Volume participation rate must be greater than 0 and at most 1."
        );

        var strategy = await _dbContext
            .Strategies.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == query.StrategyId, cancellationToken);

        Guard.Against.NotFound(query.StrategyId, strategy);
        Guard.Against.InvalidInput(
            query.MarketId,
            nameof(query.MarketId),
            m => m == strategy.MarketId,
            $"Strategy '{strategy.Id}' does not belong to market '{query.MarketId}'."
        );

        var participationRate =
            query.VolumeParticipationRate
            ?? await LatestCompletedParticipationRateAsync(strategy.Id, cancellationToken)
            ?? BacktestDataOptions.DefaultVolumeParticipationRate;

        var recommendations = await _engines
            .For(strategy.Configuration)
            .RecommendAsync(
                strategy.MarketId,
                strategy.Configuration,
                query.Budget,
                participationRate,
                cancellationToken
            );

        _logger.LogDebug(
            "Successfully handled get recommendations request. {count} recommendations.",
            recommendations.Count
        );
        return new GetRecommendationsResponse(recommendations, participationRate);
    }

    private async Task<decimal?> LatestCompletedParticipationRateAsync(
        Id<Strategy> strategyId,
        CancellationToken cancellationToken
    )
    {
        var runSettings = await _dbContext
            .Backtests.AsNoTracking()
            .Where(b => b.StrategyId == strategyId && b.Status == BacktestStatus.Completed)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => b.RunSettings)
            .FirstOrDefaultAsync(cancellationToken);

        return runSettings?.VolumeParticipationRate;
    }
}

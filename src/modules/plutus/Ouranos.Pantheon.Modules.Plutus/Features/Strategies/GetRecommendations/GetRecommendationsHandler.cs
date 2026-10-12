using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetRecommendations.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Engine;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;

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

        var recommendations = await _engines
            .For(strategy.Configuration)
            .RecommendAsync(
                strategy.MarketId,
                strategy.Configuration,
                query.Budget,
                cancellationToken
            );

        _logger.LogDebug(
            "Successfully handled get recommendations request. {count} recommendations.",
            recommendations.Count
        );
        return new GetRecommendationsResponse(recommendations);
    }
}

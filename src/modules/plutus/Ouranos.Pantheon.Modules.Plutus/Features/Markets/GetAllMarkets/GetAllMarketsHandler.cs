using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetAllMarkets.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetAllMarkets;

public sealed class GetAllMarketsHandler
    : IPantheonHandler<GetAllMarketsInput, List<GetAllMarketsResponse>>
{
    private static readonly FilterBuilder<Market> FilterBuilder = new FilterBuilder<Market>()
        .On(nameof(Market.Name), m => m.Name, caseInsensitive: true)
        .On(nameof(Market.IsForecastingEnabled), m => m.IsForecastingEnabled)
        .On(nameof(Market.Description), m => m.Description, caseInsensitive: true);

    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetAllMarketsHandler> _logger;

    public GetAllMarketsHandler(ILogger<GetAllMarketsHandler> logger, PlutusDbContext dbContext)
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<List<GetAllMarketsResponse>> Handle(
        GetAllMarketsInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get all markets query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var markets = await _dbContext
            .Markets.AsQueryable()
            .AsNoTracking()
            .FilterBy(query.Filter, FilterBuilder)
            .Select(m => new GetAllMarketsResponse(
                m.Id,
                m.Name,
                m.Taxes,
                m.IsForecastingEnabled,
                m.Description,
                m.Icon
            ))
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Successfully handled get all markets request.");
        return markets;
    }
}

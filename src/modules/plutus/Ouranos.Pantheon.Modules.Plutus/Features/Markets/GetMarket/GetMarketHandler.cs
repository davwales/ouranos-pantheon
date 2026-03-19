using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket;

public sealed class GetMarketHandler : IPantheonHandler<GetMarketInput, GetMarketResponse>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetMarketHandler> _logger;

    public GetMarketHandler(
        ILogger<GetMarketHandler> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<GetMarketResponse> Handle(
        GetMarketInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get market query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var market = await _dbContext.Markets
            .FirstOrDefaultAsync(m => m.Id == query.MarketId, cancellationToken);

        Guard.Against.NotFound(query.MarketId, market);

        _logger.LogDebug("Successfully handled get market request.");
        return new GetMarketResponse(market.Id, market.Name, market.Taxes, market.IsForecastingEnabled, market.Description, market.Icon);
    }
}

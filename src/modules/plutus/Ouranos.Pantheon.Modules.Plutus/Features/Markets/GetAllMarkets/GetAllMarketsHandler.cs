using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetAllMarkets.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetAllMarkets;

public sealed class GetAllMarketsHandler
    : QueryHandler<GetAllMarketsInput, WrapperResponse<IQueryable<Market>>>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetAllMarketsHandler> _logger;

    public GetAllMarketsHandler(
        ILogger<GetAllMarketsHandler> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public override async Task<WrapperResponse<IQueryable<Market>>> Handle(
        GetAllMarketsInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get all markets query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var queryable = _dbContext.Markets.AsQueryable();
        var response = new WrapperResponse<IQueryable<Market>>(queryable);

        _logger.LogDebug("Successfully handled get all markets request.");
        return await Task.FromResult(response);
    }
}

using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetAllForecasts.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetAllForecasts;

public sealed class GetAllForecastsHandler
    : QueryHandler<GetAllForecastsInput, WrapperResponse<IQueryable<Forecast>>>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetAllForecastsHandler> _logger;

    public GetAllForecastsHandler(
        ILogger<GetAllForecastsHandler> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public override async Task<WrapperResponse<IQueryable<Forecast>>> Handle(
        GetAllForecastsInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get all forecasts query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var queryable = _dbContext.Forecasts.AsQueryable();
        var response = new WrapperResponse<IQueryable<Forecast>>(queryable);

        _logger.LogDebug("Successfully handled get all forecasts request.");
        return await Task.FromResult(response);
    }
}

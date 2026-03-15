using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetAllTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetAllTrades;

public sealed class GetAllTradesHandler
    : QueryHandler<GetAllTradesInput, WrapperResponse<IQueryable<Trade>>>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetAllTradesHandler> _logger;

    public GetAllTradesHandler(
        ILogger<GetAllTradesHandler> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public override async Task<WrapperResponse<IQueryable<Trade>>> Handle(
        GetAllTradesInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get all trades query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var queryable = _dbContext.Trades.AsQueryable();
        var response = new WrapperResponse<IQueryable<Trade>>(queryable);

        _logger.LogDebug("Successfully handled get all trades request.");
        return await Task.FromResult(response);
    }
}

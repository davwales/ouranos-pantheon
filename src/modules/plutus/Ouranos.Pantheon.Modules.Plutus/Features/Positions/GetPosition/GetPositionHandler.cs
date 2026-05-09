using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.GetPosition.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Positions.GetPosition;

public sealed class GetPositionHandler : IPantheonHandler<GetPositionInput, GetPositionResponse>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetPositionHandler> _logger;

    public GetPositionHandler(
        ILogger<GetPositionHandler> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<GetPositionResponse> Handle(
        GetPositionInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get position query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var position = await _dbContext.Positions
            .AsNoTracking()
            .Include(p => p.Symbol)
            .FirstOrDefaultAsync(p => p.Id == query.PositionId, cancellationToken);

        Guard.Against.NotFound(query.PositionId, position);

        _logger.LogDebug("Successfully handled get position request.");
        return new GetPositionResponse(
            position.Id,
            position.Side,
            position.Status,
            position.MarketId,
            position.SymbolId,
            position.Symbol.Name,
            position.Cost,
            position.Quantity,
            position.LinkedBuyPositionId,
            position.StrategyId,
            position.Notes,
            position.CreatedAt,
            position.UpdatedAt
        );
    }
}

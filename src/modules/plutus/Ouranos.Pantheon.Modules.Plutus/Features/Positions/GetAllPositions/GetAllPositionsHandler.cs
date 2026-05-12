using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.GetAllPositions.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Pagination;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Sorting;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Positions.GetAllPositions;

public sealed class GetAllPositionsHandler
    : IPantheonHandler<GetAllPositionsInput, PagedResponse<GetAllPositionsResponse>>
{
    private static readonly FilterBuilder<GetAllPositionsResponse> FilterBuilder =
        new FilterBuilder<GetAllPositionsResponse>()
            .On(nameof(GetAllPositionsResponse.SymbolId), x => x.SymbolId)
            .On(nameof(GetAllPositionsResponse.SymbolName), x => x.SymbolName)
            .On(nameof(GetAllPositionsResponse.Side), x => x.Side)
            .On(nameof(GetAllPositionsResponse.Status), x => x.Status)
            .On(nameof(GetAllPositionsResponse.Cost), x => x.Cost)
            .On(nameof(GetAllPositionsResponse.Quantity), x => x.Quantity);

    private static readonly SortBuilder<GetAllPositionsResponse> SortBuilder =
        new SortBuilder<GetAllPositionsResponse>()
            .On(nameof(GetAllPositionsResponse.SymbolId), x => x.SymbolId)
            .On(nameof(GetAllPositionsResponse.SymbolName), x => x.SymbolName)
            .On(nameof(GetAllPositionsResponse.Side), x => x.Side)
            .On(nameof(GetAllPositionsResponse.Status), x => x.Status)
            .On(nameof(GetAllPositionsResponse.Cost), x => x.Cost)
            .On(nameof(GetAllPositionsResponse.Quantity), x => x.Quantity)
            .On(nameof(GetAllPositionsResponse.CreatedAt), x => x.CreatedAt)
            .Default(x => x.CreatedAt);

    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetAllPositionsHandler> _logger;
    private readonly IOptions<QueryOptions> _queryOptions;

    public GetAllPositionsHandler(
        ILogger<GetAllPositionsHandler> logger,
        PlutusDbContext dbContext,
        IOptions<QueryOptions> queryOptions
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);
        Guard.Against.Null(queryOptions);

        _logger = logger;
        _dbContext = dbContext;
        _queryOptions = queryOptions;
    }

    public async Task<PagedResponse<GetAllPositionsResponse>> Handle(
        GetAllPositionsInput input,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get all positions query '{@query}'.", input);
        cancellationToken.ThrowIfCancellationRequested();

        var limits = _queryOptions.Value;
        Guard.Against.OutOfRange(input.Skip, nameof(input.Skip), 0, limits.MaxSkip);
        Guard.Against.OutOfRange(
            input.Take,
            nameof(input.Take),
            limits.MinPageSize,
            limits.MaxPageSize
        );

        var query = _dbContext.Positions.AsNoTracking().Where(p => p.MarketId == input.MarketId);

        if (input.Side is not null)
        {
            query = query.Where(p => p.Side == input.Side);
        }

        if (input.Status is not null)
        {
            query = query.Where(p => p.Status == input.Status);
        }

        var positions = await query
            .Select(p => new GetAllPositionsResponse(
                p.Id,
                p.Side,
                p.Status,
                p.MarketId,
                p.SymbolId,
                p.Symbol.Name,
                p.Cost,
                p.Quantity,
                p.LinkedBuyPositionId,
                p.StrategyId,
                p.Notes,
                p.CreatedAt,
                p.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        var filtered = positions.AsQueryable().FilterBy(input.Filter, FilterBuilder);
        var totalCount = filtered.Count();

        var items = filtered
            .SortBy(input.SortField, input.SortDirection, SortBuilder)
            .Paginate(input.Skip, input.Take)
            .ToList();

        _logger.LogDebug("Successfully handled get all positions request.");
        return new PagedResponse<GetAllPositionsResponse>(
            items,
            totalCount,
            input.Skip,
            input.Take
        );
    }
}

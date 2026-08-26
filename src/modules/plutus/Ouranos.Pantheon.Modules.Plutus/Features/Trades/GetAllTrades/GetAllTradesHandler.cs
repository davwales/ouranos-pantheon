using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetAllTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Filtering;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Pagination;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Sorting;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetAllTrades;

public sealed class GetAllTradesHandler
    : IPantheonHandler<GetAllTradesInput, List<GetAllTradesResponse>>
{
    private static readonly FilterBuilder<Trade> FilterBuilder = new FilterBuilder<Trade>()
        .On(nameof(GetAllTradesResponse.MarketId), t => t.Symbol.MarketId)
        .On(nameof(GetAllTradesResponse.SymbolId), t => t.SymbolId)
        .On(nameof(GetAllTradesResponse.Price), t => t.Price)
        .On(nameof(GetAllTradesResponse.Volume), t => t.Volume)
        .On(nameof(GetAllTradesResponse.Timestamp), t => t.Timestamp);

    private static readonly SortBuilder<Trade> SortBuilder = new SortBuilder<Trade>()
        .On(nameof(GetAllTradesResponse.Timestamp), t => t.Timestamp)
        .On(nameof(GetAllTradesResponse.Price), t => t.Price)
        .Default(t => t.Timestamp);

    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetAllTradesHandler> _logger;
    private readonly IOptions<QueryOptions> _queryOptions;

    public GetAllTradesHandler(
        ILogger<GetAllTradesHandler> logger,
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

    public async Task<List<GetAllTradesResponse>> Handle(
        GetAllTradesInput input,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get all trades query '{@query}'.", input);
        cancellationToken.ThrowIfCancellationRequested();

        var limits = _queryOptions.Value;
        Guard.Against.OutOfRange(input.Skip, nameof(input.Skip), 0, limits.MaxSkip);
        Guard.Against.OutOfRange(
            input.Take,
            nameof(input.Take),
            limits.MinPageSize,
            limits.MaxPageSize
        );

        var items = await _dbContext
            .Trades.AsQueryable()
            .AsNoTracking()
            .FilterBy(input.Filter, FilterBuilder)
            .SortBy(input.SortField, input.SortDirection, SortBuilder)
            .Paginate(input.Skip, input.Take)
            .Select(t => new GetAllTradesResponse(
                t.Id,
                t.SymbolId,
                t.Symbol.MarketId,
                t.Symbol.Name,
                t.Symbol.Code,
                t.Price,
                t.Volume,
                t.Timestamp
            ))
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Successfully handled get all trades request.");
        return items;
    }
}

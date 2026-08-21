using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Pagination;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Sorting;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades;

public sealed class GetMarketTradesHandler
    : IPantheonHandler<GetMarketTradesInput, PagedResponse<GetMarketTradesResponse>>
{
    private static readonly FilterBuilder<GetMarketTradesResponse> FilterBuilder =
        new FilterBuilder<GetMarketTradesResponse>()
            .On(
                nameof(GetMarketTradesResponse.SymbolName),
                x => x.SymbolName,
                caseInsensitive: true
            )
            .On(
                nameof(GetMarketTradesResponse.SymbolSubcode),
                x => x.SymbolSubcode,
                caseInsensitive: true
            )
            .On(nameof(GetMarketTradesResponse.MinPrice), x => x.MinPrice)
            .On(nameof(GetMarketTradesResponse.MaxPrice), x => x.MaxPrice)
            .On(nameof(GetMarketTradesResponse.TotalVolume), x => x.TotalVolume)
            .On(nameof(GetMarketTradesResponse.Margin), x => x.Margin)
            .On(nameof(GetMarketTradesResponse.AveragePrice), x => x.AveragePrice)
            .On(nameof(GetMarketTradesResponse.TotalGain), x => x.TotalGain)
            .On(nameof(GetMarketTradesResponse.Roi), x => x.Roi);

    private static readonly SortBuilder<GetMarketTradesResponse> SortBuilder =
        new SortBuilder<GetMarketTradesResponse>()
            .On(nameof(GetMarketTradesResponse.TotalGain), x => x.TotalGain)
            .On(nameof(GetMarketTradesResponse.Margin), x => x.Margin)
            .On(nameof(GetMarketTradesResponse.AveragePrice), x => x.AveragePrice)
            .On(nameof(GetMarketTradesResponse.Roi), x => x.Roi)
            .On(nameof(GetMarketTradesResponse.MinPrice), x => x.MinPrice)
            .On(nameof(GetMarketTradesResponse.MaxPrice), x => x.MaxPrice)
            .On(nameof(GetMarketTradesResponse.TotalVolume), x => x.TotalVolume)
            .Default(x => x.TotalGain);

    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetMarketTradesHandler> _logger;
    private readonly IOptions<QueryOptions> _queryOptions;

    public GetMarketTradesHandler(
        ILogger<GetMarketTradesHandler> logger,
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

    public async Task<PagedResponse<GetMarketTradesResponse>> Handle(
        GetMarketTradesInput input,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle symbol statistics query '{@query}'.", input);
        cancellationToken.ThrowIfCancellationRequested();

        var limits = _queryOptions.Value;
        Guard.Against.OutOfRange(input.Skip, nameof(input.Skip), 0, limits.MaxSkip);
        Guard.Against.OutOfRange(
            input.Take,
            nameof(input.Take),
            limits.MinPageSize,
            limits.MaxPageSize
        );

        var snapshots = await (
            from s in _dbContext.MarketTradeSnapshots.AsNoTracking()
            where s.MarketId == input.MarketId && s.TimeFrame == input.TimeFrame
            join sym in _dbContext.Symbols on s.SymbolId equals sym.Id
            select new GetMarketTradesResponse(
                sym.Id,
                sym.Name,
                sym.Subcode,
                s.TotalSpent,
                s.MinPrice,
                s.MaxPrice,
                s.TotalVolume,
                s.NumTransactions,
                s.Limit,
                s.Tax
            )
        ).ToListAsync(cancellationToken);

        if (snapshots.Count == 0)
        {
            _logger.LogDebug(
                "No snapshots found for market {MarketId} and time frame {TimeFrame}.",
                input.MarketId,
                input.TimeFrame
            );
            return new PagedResponse<GetMarketTradesResponse>([], 0, input.Skip, input.Take);
        }

        var filtered = snapshots.AsQueryable().FilterBy(input.Filter, FilterBuilder);
        var totalCount = filtered.Count();

        var page = filtered
            .SortBy(input.SortField, input.SortDirection, SortBuilder)
            .Paginate(input.Skip, input.Take)
            .ToList();

        _logger.LogDebug("Successfully handled symbol statistics request.");
        return new PagedResponse<GetMarketTradesResponse>(page, totalCount, input.Skip, input.Take);
    }
}

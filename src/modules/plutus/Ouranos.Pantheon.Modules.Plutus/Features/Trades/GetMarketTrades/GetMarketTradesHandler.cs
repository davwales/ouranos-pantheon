using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Pagination;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Sorting;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades;

public sealed class GetMarketTradesHandler
    : QueryHandler<GetMarketTradesInput, PagedResponse<GetMarketTradesResponse>>
{
    private static readonly FilterBuilder<GetMarketTradesResponse> FilterBuilder =
        new FilterBuilder<GetMarketTradesResponse>()
            .On(nameof(GetMarketTradesResponse.SymbolName), x => x.SymbolName)
            .On(nameof(GetMarketTradesResponse.SymbolSubcode), x => x.SymbolSubcode)
            .On(nameof(GetMarketTradesResponse.MinPrice), x => x.MinPrice)
            .On(nameof(GetMarketTradesResponse.MaxPrice), x => x.MaxPrice)
            .On(nameof(GetMarketTradesResponse.TotalVolume), x => x.TotalVolume)
            .On(nameof(GetMarketTradesResponse.Margin), x => x.Margin)
            .On(nameof(GetMarketTradesResponse.AveragePrice), x => x.AveragePrice)
            .On(nameof(GetMarketTradesResponse.TotalGain), x => x.TotalGain)
            .On(nameof(GetMarketTradesResponse.Roi), x => x.Roi);

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

    public override async Task<PagedResponse<GetMarketTradesResponse>> Handle(
        GetMarketTradesInput input,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle symbol statistics query '{@query}'.", input);
        cancellationToken.ThrowIfCancellationRequested();

        var limits = _queryOptions.Value;
        Guard.Against.OutOfRange(input.Skip, nameof(input.Skip), 0, limits.MaxSkip);
        Guard.Against.OutOfRange(input.Take, nameof(input.Take), limits.MinPageSize, limits.MaxPageSize);

        if (input.Seconds.HasValue)
        {
            Guard.Against.NegativeOrZero(input.Seconds.Value);
        }

        var market = await _dbContext.Markets.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == input.MarketId, cancellationToken);
        Guard.Against.NotFound(input.MarketId, market);

        var flatTax = market.Taxes.Flat ?? new FlatTax(0, 0, 0);

        DateTimeOffset? since = input.Seconds.HasValue
            ? DateTimeOffset.UtcNow - TimeSpan.FromSeconds(input.Seconds.Value)
            : null;

        var results = await _dbContext.Trades
            .AsNoTracking()
            .Where(x => x.Symbol.MarketId == input.MarketId &&
                        (since == null || x.Timestamp >= since)
            )
            .GroupBy(t => t.Symbol)
            .Select(g => new
            {
                Symbol = g.Key,
                TotalSpent = g.Sum(t => t.Price * t.Volume),
                MinPrice = g.Min(t => t.Price),
                MaxPrice = g.Max(t => t.Price),
                TotalVolume = g.Sum(t => t.Volume),
                NumTransactions = g.Count(),
                Limit = g.Key.AdditionalFields.Limit ?? g.Sum(t => t.Volume)
            }
            )
            .Select(x => new GetMarketTradesResponse(
                    x.Symbol.Id,
                    x.Symbol.Name,
                    x.Symbol.Subcode,
                    x.TotalSpent,
                    x.MinPrice,
                    x.MaxPrice,
                    x.TotalVolume,
                    x.NumTransactions,
                    x.Limit,
                    x.MaxPrice * flatTax.Rate
                )
            )
            .ToListAsync(cancellationToken);

        var filtered = results.AsQueryable().FilterBy(input.Filter, FilterBuilder);
        var totalCount = filtered.Count();

        var page = filtered
            .SortBy(
                input.SortField,
                input.SortDirection,
                builder => builder
                    .On(nameof(GetMarketTradesResponse.TotalGain), x => x.TotalGain)
                    .On(nameof(GetMarketTradesResponse.Margin), x => x.Margin)
                    .On(nameof(GetMarketTradesResponse.AveragePrice), x => x.AveragePrice)
                    .On(nameof(GetMarketTradesResponse.Roi), x => x.Roi)
                    .On(nameof(GetMarketTradesResponse.MinPrice), x => x.MinPrice)
                    .On(nameof(GetMarketTradesResponse.MaxPrice), x => x.MaxPrice)
                    .On(nameof(GetMarketTradesResponse.TotalVolume), x => x.TotalVolume)
                    .Default(x => x.TotalGain)
            )
            .Paginate(input.Skip, input.Take)
            .ToList();

        _logger.LogDebug("Successfully handled symbol statistics request.");
        return new PagedResponse<GetMarketTradesResponse>(page, totalCount, input.Skip, input.Take);
    }
}

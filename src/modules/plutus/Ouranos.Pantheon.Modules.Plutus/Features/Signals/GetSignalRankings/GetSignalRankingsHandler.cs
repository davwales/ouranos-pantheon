using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Pagination;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Sorting;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSignalRankings.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSignalRankings;

public sealed class GetSignalRankingsHandler
    : IPantheonHandler<GetSignalRankingsInput, PagedResponse<GetSignalRankingsResponse>>
{
    private static readonly FilterBuilder<GetSignalRankingsResponse> FilterBuilder =
        new FilterBuilder<GetSignalRankingsResponse>()
            .On(nameof(GetSignalRankingsResponse.SymbolName), x => x.SymbolName)
            .On(nameof(GetSignalRankingsResponse.OverallScore), x => x.OverallScore)
            .On(nameof(GetSignalRankingsResponse.BuyScore), x => x.BuyScore)
            .On(nameof(GetSignalRankingsResponse.SellScore), x => x.SellScore)
            .On(nameof(GetSignalRankingsResponse.FlipScore), x => x.FlipScore)
            .On(nameof(GetSignalRankingsResponse.MerchScore), x => x.MerchScore)
            .On(nameof(GetSignalRankingsResponse.SignalCount), x => x.SignalCount);

    private static readonly SortBuilder<GetSignalRankingsResponse> SortBuilder =
        new SortBuilder<GetSignalRankingsResponse>()
            .On(nameof(GetSignalRankingsResponse.OverallScore), x => x.OverallScore)
            .On(nameof(GetSignalRankingsResponse.BuyScore), x => x.BuyScore)
            .On(nameof(GetSignalRankingsResponse.SellScore), x => x.SellScore)
            .On(nameof(GetSignalRankingsResponse.FlipScore), x => x.FlipScore)
            .On(nameof(GetSignalRankingsResponse.MerchScore), x => x.MerchScore)
            .On(nameof(GetSignalRankingsResponse.SymbolName), x => x.SymbolName)
            .On(nameof(GetSignalRankingsResponse.SignalCount), x => x.SignalCount)
            .Default(x => x.OverallScore, SortDirection.Desc);

    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetSignalRankingsHandler> _logger;
    private readonly IOptions<QueryOptions> _queryOptions;
    private readonly HashSet<SignalType> _buyTypes;
    private readonly HashSet<SignalType> _sellTypes;
    private readonly HashSet<SignalType> _flipTypes;
    private readonly HashSet<SignalType> _merchTypes;

    public GetSignalRankingsHandler(
        ILogger<GetSignalRankingsHandler> logger,
        PlutusDbContext dbContext,
        IOptions<QueryOptions> queryOptions,
        IEnumerable<ISignalComputer> computers
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);
        Guard.Against.Null(queryOptions);

        _logger = logger;
        _dbContext = dbContext;
        _queryOptions = queryOptions;

        var intentMap = BuildIntentMap(computers);
        _buyTypes = intentMap.GetValueOrDefault(InvestmentIntent.Buy, []);
        _sellTypes = intentMap.GetValueOrDefault(InvestmentIntent.Sell, []);
        _flipTypes = intentMap.GetValueOrDefault(InvestmentIntent.Flip, []);
        _merchTypes = intentMap.GetValueOrDefault(InvestmentIntent.Merch, []);
    }

    public async Task<PagedResponse<GetSignalRankingsResponse>> Handle(
        GetSignalRankingsInput input,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get signal rankings query '{@query}'.", input);
        cancellationToken.ThrowIfCancellationRequested();

        var limits = _queryOptions.Value;
        Guard.Against.OutOfRange(input.Skip, nameof(input.Skip), 0, limits.MaxSkip);
        Guard.Against.OutOfRange(input.Take, nameof(input.Take), limits.MinPageSize, limits.MaxPageSize);

        var buyTypes = _buyTypes;
        var sellTypes = _sellTypes;
        var flipTypes = _flipTypes;
        var merchTypes = _merchTypes;

        var rankings = await _dbContext.Signals
            .AsNoTracking()
            .Where(s => s.MarketId == input.MarketId)
            .GroupBy(s => new { s.SymbolId, s.Symbol.Name, s.Symbol.Subcode })
            .Select(g => new GetSignalRankingsResponse(
                g.Key.SymbolId,
                g.Key.Name,
                g.Key.Subcode,
                g.Average(x => x.Value),
                buyTypes.Count > 0
                    ? g.Average(x => buyTypes.Contains(x.Type) ? x.Value : null)
                    : null,
                sellTypes.Count > 0
                    ? g.Average(x => sellTypes.Contains(x.Type) ? x.Value : null)
                    : null,
                flipTypes.Count > 0
                    ? g.Average(x => flipTypes.Contains(x.Type) ? x.Value : null)
                    : null,
                merchTypes.Count > 0
                    ? g.Average(x => merchTypes.Contains(x.Type) ? x.Value : null)
                    : null,
                g.Count(),
                g.Count(x => x.Value > 0),
                g.Count(x => x.Value < 0)
            ))
            .ToListAsync(cancellationToken);

        var filtered = rankings.AsQueryable().FilterBy(input.Filter, FilterBuilder);
        var totalCount = filtered.Count();

        var items = filtered
            .SortBy(input.SortField, input.SortDirection, SortBuilder)
            .Paginate(input.Skip, input.Take)
            .ToList();

        _logger.LogDebug("Successfully handled get signal rankings request.");
        return new PagedResponse<GetSignalRankingsResponse>(items, totalCount, input.Skip, input.Take);
    }

    private static Dictionary<InvestmentIntent, HashSet<SignalType>> BuildIntentMap(
        IEnumerable<ISignalComputer> computers
    )
    {
        var map = new Dictionary<InvestmentIntent, HashSet<SignalType>>();
        foreach (var computer in computers)
        {
            foreach (var intent in computer.Intents)
            {
                if (!map.TryGetValue(intent, out var types))
                {
                    types = [];
                    map[intent] = types;
                }

                types.Add(computer.Type);
            }
        }
        return map;
    }
}

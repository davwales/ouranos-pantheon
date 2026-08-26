using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSignalRankings.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Filtering;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Pagination;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Sorting;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSignalRankings;

public sealed class GetSignalRankingsHandler
    : IPantheonHandler<GetSignalRankingsInput, PagedResponse<GetSignalRankingsResponse>>
{
    private static readonly FilterBuilder<GetSignalRankingsResponse> FilterBuilder =
        new FilterBuilder<GetSignalRankingsResponse>()
            .On(
                nameof(GetSignalRankingsResponse.SymbolName),
                x => x.SymbolName,
                caseInsensitive: true
            )
            .On(nameof(GetSignalRankingsResponse.DailyAveragePrice), x => x.DailyAveragePrice)
            .On(nameof(GetSignalRankingsResponse.DailyVolume), x => x.DailyVolume)
            .On(nameof(GetSignalRankingsResponse.OverallScore), x => x.OverallScore)
            .On(nameof(GetSignalRankingsResponse.BuyScore), x => x.BuyScore)
            .On(nameof(GetSignalRankingsResponse.SellScore), x => x.SellScore)
            .On(nameof(GetSignalRankingsResponse.FlipScore), x => x.FlipScore)
            .On(nameof(GetSignalRankingsResponse.MerchScore), x => x.MerchScore)
            .On(nameof(GetSignalRankingsResponse.SignalCount), x => x.SignalCount);

    private static readonly SortBuilder<GetSignalRankingsResponse> SortBuilder =
        new SortBuilder<GetSignalRankingsResponse>()
            .On(nameof(GetSignalRankingsResponse.DailyAveragePrice), x => x.DailyAveragePrice)
            .On(nameof(GetSignalRankingsResponse.DailyVolume), x => x.DailyVolume)
            .On(nameof(GetSignalRankingsResponse.OverallScore), x => x.OverallScore)
            .On(nameof(GetSignalRankingsResponse.BuyScore), x => x.BuyScore)
            .On(nameof(GetSignalRankingsResponse.SellScore), x => x.SellScore)
            .On(nameof(GetSignalRankingsResponse.FlipScore), x => x.FlipScore)
            .On(nameof(GetSignalRankingsResponse.MerchScore), x => x.MerchScore)
            .On(nameof(GetSignalRankingsResponse.SymbolName), x => x.SymbolName)
            .On(nameof(GetSignalRankingsResponse.SignalCount), x => x.SignalCount)
            .Default(x => x.OverallScore);

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
        Guard.Against.OutOfRange(
            input.Take,
            nameof(input.Take),
            limits.MinPageSize,
            limits.MaxPageSize
        );

        var symbols = await _dbContext
            .Symbols.AsNoTracking()
            .Where(s => s.MarketId == input.MarketId)
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.Subcode,
            })
            .ToDictionaryAsync(s => s.Id, cancellationToken);

        var symbolIds = symbols.Keys.ToList();

        var latestRows = await _dbContext
            .LatestSignals.AsNoTracking()
            .Where(s => symbolIds.Contains(s.SymbolId))
            .ToListAsync(cancellationToken);

        var signalRankings = latestRows
            .GroupBy(r => r.SymbolId)
            .Select(g => (Group: g, SymbolId: g.Key))
            .Where(t => symbols.ContainsKey(t.SymbolId))
            .Select(t =>
            {
                var symInfo = symbols[t.SymbolId];
                var g = t.Group;
                return new
                {
                    t.SymbolId,
                    symInfo.Name,
                    symInfo.Subcode,
                    OverallScore = g.Average(r => r.LastValue),
                    BuyScore = g.Average(r =>
                        _buyTypes.Contains(r.SignalType) ? (decimal?)r.LastValue : null
                    ),
                    SellScore = g.Average(r =>
                        _sellTypes.Contains(r.SignalType) ? (decimal?)r.LastValue : null
                    ),
                    FlipScore = g.Average(r =>
                        _flipTypes.Contains(r.SignalType) ? (decimal?)r.LastValue : null
                    ),
                    MerchScore = g.Average(r =>
                        _merchTypes.Contains(r.SignalType) ? (decimal?)r.LastValue : null
                    ),
                    SignalCount = g.Count(),
                    BullishCount = g.Count(r => r.LastValue > 0),
                    BearishCount = g.Count(r => r.LastValue < 0),
                };
            })
            .ToList();

        var snapshotLookup = await _dbContext
            .MarketTradeSnapshots.AsNoTracking()
            .Where(s => s.MarketId == input.MarketId && s.TimeFrame == TimeFrame.OneDay)
            .Select(s => new
            {
                s.SymbolId,
                s.TotalSpent,
                s.TotalVolume,
            })
            .ToDictionaryAsync(s => s.SymbolId, cancellationToken);

        var rankings = signalRankings
            .Select(r =>
            {
                snapshotLookup.TryGetValue(r.SymbolId, out var snap);
                return new GetSignalRankingsResponse(
                    r.SymbolId,
                    r.Name,
                    r.Subcode,
                    snap is { TotalVolume: > 0 } ? snap.TotalSpent / snap.TotalVolume : null,
                    snap?.TotalVolume,
                    r.OverallScore,
                    r.BuyScore,
                    r.SellScore,
                    r.FlipScore,
                    r.MerchScore,
                    r.SignalCount,
                    r.BullishCount,
                    r.BearishCount
                );
            })
            .ToList();

        var filtered = rankings.AsQueryable().FilterBy(input.Filter, FilterBuilder);
        var totalCount = filtered.Count();

        var items = filtered
            .SortBy(input.SortField, input.SortDirection, SortBuilder)
            .Paginate(input.Skip, input.Take)
            .ToList();

        _logger.LogDebug("Successfully handled get signal rankings request.");
        return new PagedResponse<GetSignalRankingsResponse>(
            items,
            totalCount,
            input.Skip,
            input.Take
        );
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

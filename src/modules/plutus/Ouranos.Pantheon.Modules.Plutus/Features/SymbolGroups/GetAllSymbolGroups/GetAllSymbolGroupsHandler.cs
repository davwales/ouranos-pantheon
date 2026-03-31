using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Pagination;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Sorting;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.GetAllSymbolGroups.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.SymbolGroups;

namespace Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.GetAllSymbolGroups;

public sealed class GetAllSymbolGroupsHandler
    : IPantheonHandler<GetAllSymbolGroupsInput, PagedResponse<GetAllSymbolGroupsResponse>>
{
    private static readonly FilterBuilder<GetAllSymbolGroupsResponse> FilterBuilder =
        new FilterBuilder<GetAllSymbolGroupsResponse>()
            .On(nameof(GetAllSymbolGroupsResponse.Name), x => x.Name)
            .On(nameof(GetAllSymbolGroupsResponse.SymbolCount), x => x.SymbolCount)
            .On(nameof(GetAllSymbolGroupsResponse.TotalVolume), x => x.TotalVolume)
            .On(nameof(GetAllSymbolGroupsResponse.TotalGain), x => x.TotalGain)
            .On(nameof(GetAllSymbolGroupsResponse.AverageRoi), x => x.AverageRoi)
            .On(nameof(GetAllSymbolGroupsResponse.AverageOverallScore), x => x.AverageOverallScore)
            .On(nameof(GetAllSymbolGroupsResponse.BullishCount), x => x.BullishCount)
            .On(nameof(GetAllSymbolGroupsResponse.BearishCount), x => x.BearishCount);

    private static readonly SortBuilder<GetAllSymbolGroupsResponse> SortBuilder =
        new SortBuilder<GetAllSymbolGroupsResponse>()
            .On(nameof(GetAllSymbolGroupsResponse.Name), x => x.Name)
            .On(nameof(GetAllSymbolGroupsResponse.SymbolCount), x => x.SymbolCount)
            .On(nameof(GetAllSymbolGroupsResponse.TotalVolume), x => x.TotalVolume)
            .On(nameof(GetAllSymbolGroupsResponse.TotalGain), x => x.TotalGain)
            .On(nameof(GetAllSymbolGroupsResponse.AverageRoi), x => x.AverageRoi)
            .On(nameof(GetAllSymbolGroupsResponse.AverageOverallScore), x => x.AverageOverallScore)
            .Default(x => x.Name, SortDirection.Asc);

    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetAllSymbolGroupsHandler> _logger;
    private readonly IOptions<QueryOptions> _queryOptions;

    public GetAllSymbolGroupsHandler(
        ILogger<GetAllSymbolGroupsHandler> logger,
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

    public async Task<PagedResponse<GetAllSymbolGroupsResponse>> Handle(
        GetAllSymbolGroupsInput input,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get all symbol groups query '{@query}'.", input);
        cancellationToken.ThrowIfCancellationRequested();

        var limits = _queryOptions.Value;
        Guard.Against.OutOfRange(input.Skip, nameof(input.Skip), 0, limits.MaxSkip);
        Guard.Against.OutOfRange(input.Take, nameof(input.Take), limits.MinPageSize, limits.MaxPageSize);

        var groups = await _dbContext.SymbolGroups
            .AsNoTracking()
            .Include(sg => sg.Members)
            .Where(sg => sg.MarketId == input.MarketId)
            .ToListAsync(cancellationToken);

        var allSymbolIds = groups
            .SelectMany(sg => sg.Members.Select(m => m.SymbolId))
            .Distinct()
            .ToList();

        var (snapshotLookup, signalLookup) = await LoadLookupsAsync(allSymbolIds, input, cancellationToken);

        var projected = groups
            .Select(sg => ProjectGroup(sg, snapshotLookup, signalLookup))
            .ToList();

        var filtered = projected.AsQueryable().FilterBy(input.Filter, FilterBuilder);
        var totalCount = filtered.Count();

        var items = filtered
            .SortBy(input.SortField, input.SortDirection, SortBuilder)
            .Paginate(input.Skip, input.Take)
            .ToList();

        _logger.LogDebug("Successfully handled get all symbol groups request.");
        return new PagedResponse<GetAllSymbolGroupsResponse>(items, totalCount, input.Skip, input.Take);
    }

    private async Task<(Dictionary<Id<Symbol>, SnapshotData>, Dictionary<Id<Symbol>, decimal>)> LoadLookupsAsync(
        List<Id<Symbol>> symbolIds,
        GetAllSymbolGroupsInput input,
        CancellationToken cancellationToken
    )
    {
        if (symbolIds.Count == 0)
        {
            return ([], []);
        }

        var snapshots = await _dbContext.MarketTradeSnapshots
            .AsNoTracking()
            .Where(s => symbolIds.Contains(s.SymbolId) && s.TimeFrame == input.TimeFrame)
            .Select(s => new SnapshotData(s.SymbolId, s.MinPrice, s.MaxPrice, s.TotalVolume, s.Limit, s.Tax))
            .ToListAsync(cancellationToken);

        var signalData = await _dbContext.Signals
            .AsNoTracking()
            .Where(s => symbolIds.Contains(s.SymbolId))
            .GroupBy(s => s.SymbolId)
            .Select(g => new { SymbolId = g.Key, AverageScore = g.Average(s => s.Value) })
            .ToListAsync(cancellationToken);

        return (
            snapshots.ToDictionary(s => s.SymbolId),
            signalData.ToDictionary(s => s.SymbolId, s => s.AverageScore)
        );
    }

    private static GetAllSymbolGroupsResponse ProjectGroup(
        SymbolGroup group,
        Dictionary<Id<Symbol>, SnapshotData> snapshotLookup,
        Dictionary<Id<Symbol>, decimal> signalLookup
    )
    {
        var memberIds = group.Members.Select(m => m.SymbolId).ToList();

        var groupSnapshots = memberIds
            .Select(id => snapshotLookup.TryGetValue(id, out var snap) ? snap : null)
            .Where(s => s is not null)
            .ToList();

        var groupSignalScores = memberIds
            .Where(id => signalLookup.ContainsKey(id))
            .Select(id => signalLookup[id])
            .ToList();

        return new GetAllSymbolGroupsResponse(
            group.Id,
            group.MarketId,
            group.Name,
            group.Description,
            SymbolCount: memberIds.Count,
            TotalVolume: groupSnapshots.Count > 0 ? groupSnapshots.Sum(s => s!.TotalVolume) : null,
            TotalGain: groupSnapshots.Count > 0
                ? groupSnapshots.Sum(s => (s!.MaxPrice - s.MinPrice - s.Tax) * (s.TotalVolume > s.Limit ? s.Limit : s.TotalVolume))
                : null,
            AverageRoi: groupSnapshots.Count > 0 && groupSnapshots.Any(s => s!.MinPrice > 0)
                ? groupSnapshots.Where(s => s!.MinPrice > 0).Average(s => (s!.MaxPrice - s.MinPrice - s.Tax) / s.MinPrice)
                : null,
            AverageOverallScore: groupSignalScores.Count > 0 ? groupSignalScores.Average() : null,
            BullishCount: groupSignalScores.Count(score => score > 0),
            BearishCount: groupSignalScores.Count(score => score < 0)
        );
    }
}

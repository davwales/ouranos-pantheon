using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.GetSymbolGroup.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.GetSymbolGroup;

public sealed class GetSymbolGroupHandler : IPantheonHandler<GetSymbolGroupInput, GetSymbolGroupResponse>
{

    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetSymbolGroupHandler> _logger;

    public GetSymbolGroupHandler(
        ILogger<GetSymbolGroupHandler> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<GetSymbolGroupResponse> Handle(
        GetSymbolGroupInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get symbol group query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var group = await _dbContext.SymbolGroups
            .AsNoTracking()
            .Include(sg => sg.Members)
                .ThenInclude(m => m.Symbol)
            .FirstOrDefaultAsync(sg => sg.Id == query.SymbolGroupId, cancellationToken);

        Guard.Against.NotFound(query.SymbolGroupId, group);

        var memberIds = group.Members.Select(m => m.SymbolId).ToList();
        var (snapshotLookup, signalLookup) = await LoadLookupsAsync(memberIds, query, cancellationToken);

        var symbols = group.Members.Select(m =>
        {
            var foundSnapshot = snapshotLookup.TryGetValue(m.SymbolId, out var snap);
            var foundSignalScore = signalLookup.TryGetValue(m.SymbolId, out var signalScore);

            return new SymbolGroupSymbolResponse(
                m.SymbolId,
                m.Symbol.Code,
                m.Symbol.Subcode,
                m.Symbol.Name,
                m.AddedAt,
                Volume: foundSnapshot ? snap.TotalVolume : null,
                Gain: foundSnapshot
                    ? (snap.MaxPrice - snap.MinPrice - snap.Tax) * (snap.TotalVolume > snap.Limit ? snap.Limit : snap.TotalVolume)
                    : null,
                Roi: foundSnapshot && snap.MinPrice > 0
                    ? (snap.MaxPrice - snap.MinPrice - snap.Tax) / snap.MinPrice
                    : null,
                SignalScore: foundSignalScore ? signalScore : null
            );
        })
        .ToList();

        _logger.LogDebug("Successfully handled get symbol group request.");
        return new GetSymbolGroupResponse(group.Id, group.MarketId, group.Name, group.Description, symbols);
    }

    private async Task<(
        Dictionary<Id<Symbol>, (decimal MinPrice, decimal MaxPrice, decimal TotalVolume, decimal Limit, decimal Tax)>,
        Dictionary<Id<Symbol>, decimal>
    )> LoadLookupsAsync(
        List<Id<Symbol>> memberIds,
        GetSymbolGroupInput query,
        CancellationToken cancellationToken
    )
    {
        if (memberIds.Count == 0)
        {
            return ([], []);
        }

        var snapshots = await _dbContext.MarketTradeSnapshots
            .AsNoTracking()
            .Where(s => memberIds.Contains(s.SymbolId) && s.TimeFrame == query.TimeFrame)
            .ToDictionaryAsync(
                s => s.SymbolId,
                s => (s.MinPrice, s.MaxPrice, s.TotalVolume, s.Limit, s.Tax),
                cancellationToken
            );

        var signals = await _dbContext.Signals
            .AsNoTracking()
            .Where(s => memberIds.Contains(s.SymbolId))
            .GroupBy(s => s.SymbolId)
            .Select(g => new { SymbolId = g.Key, AverageScore = g.Average(s => s.Value) })
            .ToDictionaryAsync(s => s.SymbolId, s => s.AverageScore, cancellationToken);

        return (snapshots, signals);
    }
}

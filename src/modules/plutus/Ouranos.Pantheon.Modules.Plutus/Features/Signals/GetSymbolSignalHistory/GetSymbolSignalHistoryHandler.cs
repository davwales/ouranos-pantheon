using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignalHistory.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Extensions;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Querying;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignalHistory;

public class GetSymbolSignalHistoryHandler
    : IPantheonHandler<GetSymbolSignalHistoryInput, GetSymbolSignalHistoryResponse>
{
    private readonly ILogger<GetSymbolSignalHistoryHandler> _logger;
    private readonly PlutusDbContext _dbContext;
    private readonly IReadOnlyDictionary<SignalType, ISignalComputer> _computers;

    public GetSymbolSignalHistoryHandler(
        ILogger<GetSymbolSignalHistoryHandler> logger,
        PlutusDbContext dbContext,
        IEnumerable<ISignalComputer> computers
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);
        Guard.Against.Null(computers);

        _logger = logger;
        _dbContext = dbContext;
        _computers = computers.ToDictionary(c => c.Type);
    }

    public async Task<GetSymbolSignalHistoryResponse> Handle(
        GetSymbolSignalHistoryInput input,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Fetching signal history for symbol {SymbolId}", input.SymbolId);
        cancellationToken.ThrowIfCancellationRequested();

        var symbol = await _dbContext.Symbols.FindAsync([input.SymbolId], cancellationToken);
        Guard.Against.NotFound(input.SymbolId, symbol);

        var (effectiveFrom, effectiveTo) = ResolveDateRange(input.From, input.To);
        var requestedTypes = ParseTypeFilter(input.Types);
        var typesForIntent = ResolveIntentTypeFilter(input.Intent);

        var signalGroups = await FetchSignalGroups(
            input.SymbolId,
            effectiveFrom,
            effectiveTo,
            requestedTypes,
            typesForIntent,
            cancellationToken
        );

        var signalResponses = BuildSignalResponses(signalGroups);
        var summary = ComputeSummary(signalResponses);
        AddMissingSignalTypes(signalResponses, signalGroups, requestedTypes, typesForIntent);

        _logger.LogDebug(
            "Returning signal history for {SymbolName} ({SignalCount} signal types, {WindowStart} to {WindowEnd})",
            symbol.Name,
            signalResponses.Count,
            effectiveFrom,
            effectiveTo
        );

        return new GetSymbolSignalHistoryResponse(
            input.SymbolId,
            symbol.Name,
            signalResponses,
            summary
        );
    }

    private static (DateTimeOffset From, DateTimeOffset To) ResolveDateRange(
        DateTimeOffset? from,
        DateTimeOffset? to
    )
    {
        var effectiveFrom = from ?? DateTimeOffset.UtcNow.AddDays(-7);
        var effectiveTo = to ?? DateTimeOffset.UtcNow;

        if (effectiveFrom > effectiveTo)
        {
            throw new ArgumentException("'from' must be before 'to'");
        }

        return (effectiveFrom, effectiveTo);
    }

    private static HashSet<SignalType>? ParseTypeFilter(string? types)
    {
        if (string.IsNullOrWhiteSpace(types))
        {
            return null;
        }

        var filter = new HashSet<SignalType>();
        foreach (
            var typeName in types.Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            )
        )
        {
            if (!Enum.TryParse<SignalType>(typeName, true, out var parsedType))
            {
                throw new ArgumentException($"Unknown signal type: '{typeName}'");
            }
            filter.Add(parsedType);
        }

        return filter;
    }

    private HashSet<SignalType>? ResolveIntentTypeFilter(InvestmentIntent? intent)
    {
        if (intent is null)
        {
            return null;
        }

        return
        [
            .. _computers
                .Values.Where(c => c.Intents.Any(ci => (ci & intent) != 0))
                .Select(c => c.Type),
        ];
    }

    protected internal virtual async Task<
        Dictionary<SignalType, List<RawSignalPoint>>
    > FetchSignalGroups(
        Id<Symbol> symbolId,
        DateTimeOffset from,
        DateTimeOffset to,
        HashSet<SignalType>? requestedTypes,
        HashSet<SignalType>? typesForIntent,
        CancellationToken ct
    )
    {
        var requested = requestedTypes is { Count: > 0 } r ? r : null;
        var intent = typesForIntent is { Count: > 0 } i ? i : null;

        IEnumerable<SignalType> selected = (requested, intent) switch
        {
            ({ } req, { } it) => req.Intersect(it),
            ({ } req, null) => req,
            (null, { } it) => it,
            _ => Enum.GetValues<SignalType>(),
        };

        int[] effectiveTypes = [.. selected.Select(t => (int)t)];

        var alignedFrom = new DateTimeOffset(
            from.Ticks - (from.Ticks % (TimeSpan.TicksPerMinute * 30)),
            from.Offset
        );

        var command = RawSqlCommand
            .FromSql(
                """
                SELECT time_bucket_gapfill('30 minutes'::interval, bucket, @from, @to) AS bucket,
                       signal_type,
                       locf(last(last_value, bucket)) AS value
                FROM plutus.signal_history_30m
                WHERE symbol_id = @symbolId
                  AND bucket >= @from
                  AND bucket < @to
                  AND signal_type = ANY(@types)
                GROUP BY time_bucket_gapfill('30 minutes'::interval, bucket, @from, @to), signal_type
                ORDER BY signal_type, bucket
                """
            )
            .WithId("@symbolId", symbolId)
            .WithDateTimeOffset("@from", alignedFrom)
            .WithDateTimeOffset("@to", to)
            .WithParameter(
                new NpgsqlParameter("@types", NpgsqlDbType.Array | NpgsqlDbType.Integer)
                {
                    Value = effectiveTypes,
                }
            );

        var rows = await _dbContext.Database.ExecuteQueryAsync<SignalHistoryGapfillRow>(
            command,
            ct
        );

        return rows.Where(r => r.Value.HasValue && r.Bucket >= from)
            .GroupBy(r => r.SignalType)
            .ToDictionary(
                g => (SignalType)g.Key,
                g =>
                    g.Select(r => new RawSignalPoint(
                            (SignalType)r.SignalType,
                            r.Value.GetValueOrDefault(),
                            r.Bucket
                        ))
                        .ToList()
            );
    }

    private List<SignalHistoryResponse> BuildSignalResponses(
        Dictionary<SignalType, List<RawSignalPoint>> signalGroups
    )
    {
        var responses = new List<SignalHistoryResponse>();

        foreach (var (signalType, points) in signalGroups)
        {
            if (points.Count == 0)
            {
                continue;
            }

            _computers.TryGetValue(signalType, out var computer);

            var history = points
                .Select(p => new SignalHistoryPoint(p.Value, p.ComputedAt))
                .ToList();

            var currentValue = history[^1].Value;
            var (direction, strength) = ComputeDirectionAndStrength(currentValue);

            responses.Add(
                new SignalHistoryResponse(
                    signalType.ToString(),
                    computer?.Label ?? signalType.ToString(),
                    computer?.Description ?? string.Empty,
                    computer?.Intents ?? [],
                    currentValue,
                    direction,
                    strength,
                    history
                )
            );
        }

        return responses;
    }

    private static SignalSummary ComputeSummary(List<SignalHistoryResponse> signalResponses)
    {
        if (signalResponses.Count == 0)
        {
            return new SignalSummary(0m, 0, 0, 0, false, false);
        }

        var aggregatedScore = Math.Round(signalResponses.Average(s => s.CurrentValue), 2);
        var bullishCount = signalResponses.Count(s => s.Direction == SignalDirection.Bullish);
        var bearishCount = signalResponses.Count(s => s.Direction == SignalDirection.Bearish);
        var neutralCount = signalResponses.Count(s => s.Direction == SignalDirection.Neutral);

        var flipSignals = signalResponses
            .Where(s => s.Intents.Contains(InvestmentIntent.Flip))
            .ToList();
        var isFlipFavourable = flipSignals.Count > 0 && flipSignals.All(s => s.CurrentValue > 0);

        var isMerchFavourable = signalResponses.Any(s =>
            s.Intents.Contains(InvestmentIntent.Merch) && s.CurrentValue > 0
        );

        return new SignalSummary(
            aggregatedScore,
            bullishCount,
            bearishCount,
            neutralCount,
            isFlipFavourable,
            isMerchFavourable
        );
    }

    private void AddMissingSignalTypes(
        List<SignalHistoryResponse> signalResponses,
        Dictionary<SignalType, List<RawSignalPoint>> signalGroups,
        HashSet<SignalType>? requestedTypes,
        HashSet<SignalType>? typesForIntent
    )
    {
        foreach (var (signalType, computer) in _computers)
        {
            if (signalGroups.ContainsKey(signalType))
            {
                continue;
            }

            if (requestedTypes is not null && !requestedTypes.Contains(signalType))
            {
                continue;
            }

            if (typesForIntent is not null && !typesForIntent.Contains(signalType))
            {
                continue;
            }

            signalResponses.Add(
                new SignalHistoryResponse(
                    signalType.ToString(),
                    computer.Label,
                    computer.Description,
                    computer.Intents,
                    0m,
                    SignalDirection.Neutral,
                    SignalStrength.Weak,
                    []
                )
            );
        }
    }

    private static (SignalDirection Direction, SignalStrength Strength) ComputeDirectionAndStrength(
        decimal value
    )
    {
        return value switch
        {
            > 0.67m => (SignalDirection.Bullish, SignalStrength.Strong),
            > 0.33m => (SignalDirection.Bullish, SignalStrength.Medium),
            > 0 => (SignalDirection.Bullish, SignalStrength.Weak),
            0 => (SignalDirection.Neutral, SignalStrength.Weak),
            < -0.67m => (SignalDirection.Bearish, SignalStrength.Strong),
            < -0.33m => (SignalDirection.Bearish, SignalStrength.Medium),
            _ => (SignalDirection.Bearish, SignalStrength.Weak),
        };
    }

    protected internal sealed record RawSignalPoint(
        SignalType Type,
        decimal Value,
        DateTimeOffset ComputedAt
    );
}

using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Trading;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Extensions;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Querying;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;

/// <summary>
///     Side-aware markets store each 5-minute bucket as two unlabelled trades: the average
///     buy-side price (high) and the average sell-side price (low). Pairing the two rows of a bucket recovers
///     both sides; buckets where only one side traded are ambiguous and skipped.
/// </summary>
public sealed class FlipDataQueryService : IFlipDataQueryService
{
    private readonly ILogger<FlipDataQueryService> _logger;
    private readonly IDbContextFactory<PlutusDbContext> _dbContextFactory;
    private readonly IOptions<FlipDataOptions> _options;

    public FlipDataQueryService(
        ILogger<FlipDataQueryService> logger,
        IDbContextFactory<PlutusDbContext> dbContextFactory,
        IOptions<FlipDataOptions> options
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContextFactory);
        Guard.Against.Null(options);

        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _options = options;
    }

    public async Task<FlipMarketData> LoadAsync(
        Id<Market> marketId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken
    )
    {
        var start = new DateTimeOffset(
            from.UtcDateTime.Date.AddHours(from.UtcDateTime.Hour),
            TimeSpan.Zero
        );

        var hours = Math.Max(0, (int)Math.Ceiling((to - start).TotalHours));
        var barCount = hours * FlipMarketData.BarsPerHour;

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        dbContext.Database.SetCommandTimeout(
            TimeSpan.FromMinutes(_options.Value.QueryTimeoutMinutes)
        );

        var market = await dbContext
            .Markets.AsNoTracking()
            .FirstAsync(m => m.Id == marketId, cancellationToken);

        var universe = await LoadUniverseAsync(dbContext, marketId, start, cancellationToken);
        var bars = universe.ToDictionary(u => u.Id, _ => new FlipBar[barCount]);

        var command = RawSqlCommand
            .FromSql(
                """
                SELECT symbol_id,
                       "timestamp",
                       MAX(price) AS high_price,
                       last(volume, price) AS high_volume,
                       MIN(price) AS low_price,
                       first(volume, price) AS low_volume
                FROM plutus.trades
                WHERE symbol_id = ANY(@symbolIds)
                  AND "timestamp" >= @from
                  AND "timestamp" < @to
                GROUP BY symbol_id, "timestamp"
                HAVING COUNT(*) = 2
                """
            )
            .WithIds("@symbolIds", universe.Select(u => new Id<Symbol>(u.Id.ToString())))
            .WithDateTimeOffset("@from", start)
            .WithDateTimeOffset("@to", to);

        var rowCount = 0;
        await foreach (
            var row in dbContext
                .Database.SqlQueryRaw<FlipBarRow>(command.Sql, command.GetParameters())
                .AsAsyncEnumerable()
                .WithCancellation(cancellationToken)
        )
        {
            var index = (int)((row.Timestamp - start).TotalMinutes / FlipMarketData.BarMinutes);
            if (index < 0 || index >= barCount || !bars.TryGetValue(row.SymbolId, out var series))
            {
                continue;
            }

            series[index] = new FlipBar(
                (float)row.HighPrice,
                (float)row.HighVolume,
                (float)row.LowPrice,
                (float)row.LowVolume
            );
            rowCount++;
        }

        _logger.LogDebug(
            "Loaded flip data: {symbolCount} symbols, {barCount} paired 5-minute buckets.",
            universe.Count,
            rowCount
        );

        return new FlipMarketData(
            start,
            barCount,
            [
                .. universe.Select(u => new FlipSeries(
                    new Id<Symbol>(u.Id.ToString()),
                    u.Name,
                    u.Subcode,
                    u.BuyLimit,
                    bars[u.Id]
                )),
            ],
            MarketRules.For(market)
        );
    }

    private async Task<List<FlipUniverseRow>> LoadUniverseAsync(
        PlutusDbContext dbContext,
        Id<Market> marketId,
        DateTimeOffset start,
        CancellationToken cancellationToken
    )
    {
        var options = _options.Value;
        var command = RawSqlCommand
            .FromSql(
                """
                SELECT s.id,
                       s.name,
                       s.subcode,
                       s.additional_fields_limit AS buy_limit
                FROM plutus.symbols s
                JOIN plutus.trades_daily td ON td.symbol_id = s.id
                WHERE s.market_id = @marketId
                  AND s.additional_fields_limit > 0
                  AND td.bucket >= @universeStart
                  AND td.bucket < @universeEnd
                GROUP BY s.id, s.name, s.subcode, s.additional_fields_limit
                HAVING COUNT(*) >= @minDays
                   AND percentile_cont(0.5) WITHIN GROUP (ORDER BY td.total_spent) >= @minDailyValue
                """
            )
            .WithId("@marketId", marketId)
            .WithDateTimeOffset("@universeStart", start.AddDays(-options.UniverseDays))
            .WithDateTimeOffset("@universeEnd", start)
            .WithInt("@minDays", Math.Max(1, options.UniverseDays / 2))
            .WithDecimal("@minDailyValue", options.MinDailyValue);

        return await dbContext.Database.ExecuteQueryAsync<FlipUniverseRow>(
            command,
            cancellationToken
        );
    }
}

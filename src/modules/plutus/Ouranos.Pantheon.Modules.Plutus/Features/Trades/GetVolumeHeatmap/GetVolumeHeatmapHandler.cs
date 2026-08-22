using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetVolumeHeatmap.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Extensions;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Querying;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetVolumeHeatmap;

public sealed class GetVolumeHeatmapHandler
    : IPantheonHandler<GetVolumeHeatmapInput, GetVolumeHeatmapResponse>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetVolumeHeatmapHandler> _logger;

    public GetVolumeHeatmapHandler(
        ILogger<GetVolumeHeatmapHandler> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<GetVolumeHeatmapResponse> Handle(
        GetVolumeHeatmapInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get volume heatmap query '{@Query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var lookbackWeeks = Math.Clamp(query.LookbackWeeks, 1, 13);
        var lookbackDays = lookbackWeeks * 7;

        var command = RawSqlCommand
            .FromSql(
                """
                SELECT
                    ((EXTRACT(DOW FROM t.timestamp)::int + 6) % 7) AS day_of_week,
                    EXTRACT(HOUR FROM t.timestamp)::int AS hour,
                    COUNT(*)::bigint AS total_trades
                FROM plutus.trades t
                JOIN plutus.symbols s ON t.symbol_id = s.id
                WHERE s.market_id = @marketId
                  AND t.timestamp >= @since
                GROUP BY 1, 2
                ORDER BY 1, 2
                """
            )
            .WithId("marketId", query.MarketId)
            .WithDateTimeOffset("since", DateTimeOffset.UtcNow.AddDays(-lookbackDays));

        var rows = await _dbContext.Database.ExecuteQueryAsync<HeatmapRow>(
            command,
            cancellationToken
        );

        if (rows.Count == 0)
        {
            _logger.LogDebug(
                "No volume heatmap data found for market '{MarketId}'.",
                query.MarketId
            );
            return new GetVolumeHeatmapResponse([]);
        }

        var cells = ComputeCells(rows);

        _logger.LogDebug("Successfully handled get volume heatmap request.");
        return new GetVolumeHeatmapResponse(cells);
    }

    internal static List<HeatmapCellResponse> ComputeCells(List<HeatmapRow> rows)
    {
        var globalTotal = rows.Sum(r => r.TotalTrades);

        return
        [
            .. rows.Select(r => new HeatmapCellResponse(
                r.DayOfWeek,
                r.Hour,
                r.TotalTrades,
                Math.Round((decimal)r.TotalTrades / globalTotal * 100m, 2)
            )),
        ];
    }
}

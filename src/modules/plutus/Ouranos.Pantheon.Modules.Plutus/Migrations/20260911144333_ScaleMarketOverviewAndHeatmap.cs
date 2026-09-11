using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Migrations;

/// <inheritdoc />
public partial class ScaleMarketOverviewAndHeatmap : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            CREATE MATERIALIZED VIEW plutus.trades_market_hourly
            WITH (timescaledb.continuous, timescaledb.materialized_only = false) AS
            SELECT
                time_bucket(INTERVAL '1 hour', t."timestamp") AS bucket,
                s.market_id,
                SUM(t.price * t.volume) AS total_spent,
                SUM(t.volume) AS volume,
                COUNT(*) AS num_transactions
            FROM plutus.trades t
            JOIN plutus.symbols s ON s.id = t.symbol_id
            GROUP BY bucket, s.market_id
            WITH NO DATA;

            SELECT add_continuous_aggregate_policy('plutus.trades_market_hourly',
                start_offset => INTERVAL '97 days',
                end_offset => INTERVAL '1 hour',
                schedule_interval => INTERVAL '15 minutes',
                if_not_exists => TRUE);
            """
        );

        migrationBuilder.Sql(BuildMarketOverviewBucketsView());
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DROP VIEW IF EXISTS plutus.market_overview_buckets CASCADE;

            DROP MATERIALIZED VIEW IF EXISTS plutus.trades_market_hourly CASCADE;
            """
        );
    }

    private static string BuildMarketOverviewBucketsView()
    {
        return $$"""
            CREATE OR REPLACE VIEW plutus.market_overview_buckets AS
            {{DailyAggregateBranch("OneWeek", "7 days")}}

            UNION ALL

            {{DailyAggregateBranch("OneMonth", "30 days")}}

            UNION ALL

            {{DailyAggregateBranch("SixMonths", "182 days")}}

            UNION ALL

            {{DailyAggregateBranch("OneYear", "365 days")}}

            UNION ALL

            {{DailyAggregateBranch("AllTime", null)}}

            UNION ALL

            {{RawTradesBranch("FifteenMinutes", "15 minutes")}}

            UNION ALL

            {{RawTradesBranch("OneHour", "1 hour")}}

            UNION ALL

            {{RawTradesBranch("FourHours", "4 hours")}}

            UNION ALL

            {{RawTradesBranch("OneDay", "1 day")}};
            """;
    }

    private static string DailyAggregateBranch(string timeFrame, string window)
    {
        var boundsWindow = window is null
            ? string.Empty
            : $"\n      AND d2.day >= now() - INTERVAL '{window}'";
        var joinWindow = window is null
            ? string.Empty
            : $"\n    AND d.day >= now() - INTERVAL '{window}'";

        return $"""
            SELECT
                m.id AS market_id,
                '{timeFrame}' AS time_frame,
                time_bucket(bounds.bucket_interval, d.day) AS bucket_start,
                SUM(d.total_spent) AS total_spent,
                SUM(d.volume) AS volume,
                SUM(d.num_transactions) AS num_transactions,
                SUM(d.total_spent) / NULLIF(SUM(d.volume), 0) AS average_price
            FROM plutus.markets m
            JOIN LATERAL (
                SELECT GREATEST(
                    plutus.smart_interval(MAX(d2.day) - MIN(d2.day), 100),
                    INTERVAL '1 day'
                ) AS bucket_interval
                FROM plutus.trades_market_daily d2
                WHERE d2.market_id = m.id{boundsWindow}
                HAVING COUNT(*) > 0 AND MAX(d2.day) > MIN(d2.day)
            ) bounds ON TRUE
            JOIN plutus.trades_market_daily d ON d.market_id = m.id{joinWindow}
            GROUP BY m.id, bounds.bucket_interval, time_bucket(bounds.bucket_interval, d.day)
            """;
    }

    private static string RawTradesBranch(string timeFrame, string window)
    {
        return $"""
            SELECT
                m.id AS market_id,
                '{timeFrame}' AS time_frame,
                time_bucket(plutus.smart_interval(INTERVAL '{window}', 100), t."timestamp") AS bucket_start,
                SUM(t.price * t.volume) AS total_spent,
                SUM(t.volume) AS volume,
                COUNT(*) AS num_transactions,
                SUM(t.price * t.volume) / NULLIF(SUM(t.volume), 0) AS average_price
            FROM plutus.markets m
            JOIN plutus.symbols s ON s.market_id = m.id
            JOIN plutus.trades t ON t.symbol_id = s.id
                AND t."timestamp" >= now() - INTERVAL '{window}'
            GROUP BY m.id, time_bucket(plutus.smart_interval(INTERVAL '{window}', 100), t."timestamp")
            """;
    }
}

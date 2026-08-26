using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Migrations;

/// <inheritdoc />
public partial class OptimizeMarketTradeSnapshotView : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            CREATE OR REPLACE VIEW plutus.market_trade_snapshots AS
            SELECT
                s.market_id                       AS market_id,
                d.symbol_id                       AS symbol_id,
                'OneWeek'                         AS time_frame,
                SUM(d.total_spent)                AS total_spent,
                MIN(d.min_price)                  AS min_price,
                MAX(d.max_price)                  AS max_price,
                SUM(d.volume)                     AS total_volume,
                SUM(d.num_transactions)           AS num_transactions,
                COALESCE(s.additional_fields_limit, SUM(d.volume)) AS "limit",
                MAX(d.max_price) * COALESCE(m.taxes_flat_rate, 0) AS tax
            FROM plutus.trades_daily d
            JOIN plutus.symbols s ON s.id = d.symbol_id
            JOIN plutus.markets m ON m.id = s.market_id
            WHERE d.bucket >= now() - INTERVAL '7 days'
            GROUP BY s.market_id, d.symbol_id, s.additional_fields_limit, m.taxes_flat_rate

            UNION ALL

            SELECT s.market_id, d.symbol_id, 'OneMonth',
                   SUM(d.total_spent), MIN(d.min_price), MAX(d.max_price), SUM(d.volume), SUM(d.num_transactions),
                   COALESCE(s.additional_fields_limit, SUM(d.volume)),
                   MAX(d.max_price) * COALESCE(m.taxes_flat_rate, 0)
            FROM plutus.trades_daily d
            JOIN plutus.symbols s ON s.id = d.symbol_id
            JOIN plutus.markets m ON m.id = s.market_id
            WHERE d.bucket >= now() - INTERVAL '30 days'
            GROUP BY s.market_id, d.symbol_id, s.additional_fields_limit, m.taxes_flat_rate

            UNION ALL

            SELECT s.market_id, d.symbol_id, 'SixMonths',
                   SUM(d.total_spent), MIN(d.min_price), MAX(d.max_price), SUM(d.volume), SUM(d.num_transactions),
                   COALESCE(s.additional_fields_limit, SUM(d.volume)), MAX(d.max_price) * COALESCE(m.taxes_flat_rate, 0)
            FROM plutus.trades_daily d
            JOIN plutus.symbols s ON s.id = d.symbol_id
            JOIN plutus.markets m ON m.id = s.market_id
            WHERE d.bucket >= now() - INTERVAL '182 days'
            GROUP BY s.market_id, d.symbol_id, s.additional_fields_limit, m.taxes_flat_rate

            UNION ALL

            SELECT s.market_id, d.symbol_id, 'OneYear',
                   SUM(d.total_spent), MIN(d.min_price), MAX(d.max_price), SUM(d.volume), SUM(d.num_transactions),
                   COALESCE(s.additional_fields_limit, SUM(d.volume)), MAX(d.max_price) * COALESCE(m.taxes_flat_rate, 0)
            FROM plutus.trades_daily d
            JOIN plutus.symbols s ON s.id = d.symbol_id
            JOIN plutus.markets m ON m.id = s.market_id
            WHERE d.bucket >= now() - INTERVAL '365 days'
            GROUP BY s.market_id, d.symbol_id, s.additional_fields_limit, m.taxes_flat_rate

            UNION ALL

            SELECT s.market_id, d.symbol_id, 'AllTime',
                    SUM(d.total_spent), MIN(d.min_price), MAX(d.max_price), SUM(d.volume), SUM(d.num_transactions),
                    COALESCE(s.additional_fields_limit, SUM(d.volume)), MAX(d.max_price) * COALESCE(m.taxes_flat_rate, 0)
            FROM plutus.trades_daily d
            JOIN plutus.symbols s ON s.id = d.symbol_id
            JOIN plutus.markets m ON m.id = s.market_id
            GROUP BY s.market_id, d.symbol_id, s.additional_fields_limit, m.taxes_flat_rate

            UNION ALL

            SELECT
                s.market_id                       AS market_id,
                s.id                              AS symbol_id,
                'FifteenMinutes'                  AS time_frame,
                agg.total_spent                   AS total_spent,
                agg.min_price                     AS min_price,
                agg.max_price                     AS max_price,
                agg.total_volume                  AS total_volume,
                agg.num_transactions              AS num_transactions,
                COALESCE(s.additional_fields_limit, agg.total_volume) AS "limit",
                agg.max_price * COALESCE(m.taxes_flat_rate, 0) AS tax
            FROM plutus.symbols s
            JOIN LATERAL (
                SELECT
                    SUM(t.price * t.volume) AS total_spent,
                    MIN(t.price)            AS min_price,
                    MAX(t.price)            AS max_price,
                    SUM(t.volume)           AS total_volume,
                    COUNT(*)                AS num_transactions
                FROM plutus.trades t
                WHERE t.symbol_id = s.id
                  AND t.timestamp >= now() - INTERVAL '15 minutes'
                HAVING COUNT(*) > 0
            ) agg ON TRUE
            JOIN plutus.markets  m ON m.id = s.market_id

            UNION ALL

            SELECT
                s.market_id                       AS market_id,
                s.id                              AS symbol_id,
                'OneHour'                         AS time_frame,
                agg.total_spent                   AS total_spent,
                agg.min_price                     AS min_price,
                agg.max_price                     AS max_price,
                agg.total_volume                  AS total_volume,
                agg.num_transactions              AS num_transactions,
                COALESCE(s.additional_fields_limit, agg.total_volume) AS "limit",
                agg.max_price * COALESCE(m.taxes_flat_rate, 0) AS tax
            FROM plutus.symbols s
            JOIN LATERAL (
                SELECT
                    SUM(t.price * t.volume) AS total_spent,
                    MIN(t.price)            AS min_price,
                    MAX(t.price)            AS max_price,
                    SUM(t.volume)           AS total_volume,
                    COUNT(*)                AS num_transactions
                FROM plutus.trades t
                WHERE t.symbol_id = s.id
                  AND t.timestamp >= now() - INTERVAL '1 hour'
                HAVING COUNT(*) > 0
            ) agg ON TRUE
            JOIN plutus.markets  m ON m.id = s.market_id

            UNION ALL

            SELECT
                s.market_id                       AS market_id,
                s.id                              AS symbol_id,
                'FourHours'                       AS time_frame,
                agg.total_spent                   AS total_spent,
                agg.min_price                     AS min_price,
                agg.max_price                     AS max_price,
                agg.total_volume                  AS total_volume,
                agg.num_transactions              AS num_transactions,
                COALESCE(s.additional_fields_limit, agg.total_volume) AS "limit",
                agg.max_price * COALESCE(m.taxes_flat_rate, 0) AS tax
            FROM plutus.symbols s
            JOIN LATERAL (
                SELECT
                    SUM(t.price * t.volume) AS total_spent,
                    MIN(t.price)            AS min_price,
                    MAX(t.price)            AS max_price,
                    SUM(t.volume)           AS total_volume,
                    COUNT(*)                AS num_transactions
                FROM plutus.trades t
                WHERE t.symbol_id = s.id
                  AND t.timestamp >= now() - INTERVAL '4 hours'
                HAVING COUNT(*) > 0
            ) agg ON TRUE
            JOIN plutus.markets  m ON m.id = s.market_id

            UNION ALL

            SELECT
                s.market_id                       AS market_id,
                s.id                              AS symbol_id,
                'OneDay'                          AS time_frame,
                agg.total_spent                   AS total_spent,
                agg.min_price                     AS min_price,
                agg.max_price                     AS max_price,
                agg.total_volume                  AS total_volume,
                agg.num_transactions              AS num_transactions,
                COALESCE(s.additional_fields_limit, agg.total_volume) AS "limit",
                agg.max_price * COALESCE(m.taxes_flat_rate, 0) AS tax
            FROM plutus.symbols s
            JOIN LATERAL (
                SELECT
                    SUM(t.price * t.volume) AS total_spent,
                    MIN(t.price)            AS min_price,
                    MAX(t.price)            AS max_price,
                    SUM(t.volume)           AS total_volume,
                    COUNT(*)                AS num_transactions
                FROM plutus.trades t
                WHERE t.symbol_id = s.id
                  AND t.timestamp >= now() - INTERVAL '1 day'
                HAVING COUNT(*) > 0
            ) agg ON TRUE
            JOIN plutus.markets  m ON m.id = s.market_id;
            """
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            CREATE OR REPLACE VIEW plutus.market_trade_snapshots AS
            SELECT
                s.market_id                       AS market_id,
                s.id                              AS symbol_id,
                tf.time_frame                     AS time_frame,
                agg.total_spent                   AS total_spent,
                agg.min_price                     AS min_price,
                agg.max_price                     AS max_price,
                agg.total_volume                  AS total_volume,
                agg.num_transactions              AS num_transactions,
                COALESCE(s.additional_fields_limit, agg.total_volume) AS "limit",
                agg.max_price * COALESCE(m.taxes_flat_rate, 0) AS tax
            FROM (VALUES
                ('OneWeek',   INTERVAL '7 days'),
                ('OneMonth',  INTERVAL '30 days'),
                ('SixMonths', INTERVAL '182 days'),
                ('OneYear',   INTERVAL '365 days'),
                ('AllTime',   NULL::interval)
            ) tf(time_frame, since)
            JOIN plutus.symbols s ON TRUE
            JOIN LATERAL (
                SELECT
                    SUM(d.total_spent)      AS total_spent,
                    MIN(d.min_price)        AS min_price,
                    MAX(d.max_price)        AS max_price,
                    SUM(d.volume)           AS total_volume,
                    SUM(d.num_transactions) AS num_transactions
                FROM plutus.trades_daily d
                WHERE d.symbol_id = s.id
                  AND d.bucket >= COALESCE(now() - tf.since, '-infinity'::timestamptz)
                HAVING COUNT(*) > 0
            ) agg ON TRUE
            JOIN plutus.markets  m ON m.id = s.market_id

            UNION ALL

            SELECT
                s.market_id                       AS market_id,
                s.id                              AS symbol_id,
                'FifteenMinutes'                  AS time_frame,
                agg.total_spent                   AS total_spent,
                agg.min_price                     AS min_price,
                agg.max_price                     AS max_price,
                agg.total_volume                  AS total_volume,
                agg.num_transactions              AS num_transactions,
                COALESCE(s.additional_fields_limit, agg.total_volume) AS "limit",
                agg.max_price * COALESCE(m.taxes_flat_rate, 0) AS tax
            FROM plutus.symbols s
            JOIN LATERAL (
                SELECT
                    SUM(t.price * t.volume) AS total_spent,
                    MIN(t.price)            AS min_price,
                    MAX(t.price)            AS max_price,
                    SUM(t.volume)           AS total_volume,
                    COUNT(*)                AS num_transactions
                FROM plutus.trades t
                WHERE t.symbol_id = s.id
                  AND t.timestamp >= now() - INTERVAL '15 minutes'
                HAVING COUNT(*) > 0
            ) agg ON TRUE
            JOIN plutus.markets  m ON m.id = s.market_id

            UNION ALL

            SELECT
                s.market_id                       AS market_id,
                s.id                              AS symbol_id,
                'OneHour'                         AS time_frame,
                agg.total_spent                   AS total_spent,
                agg.min_price                     AS min_price,
                agg.max_price                     AS max_price,
                agg.total_volume                  AS total_volume,
                agg.num_transactions              AS num_transactions,
                COALESCE(s.additional_fields_limit, agg.total_volume) AS "limit",
                agg.max_price * COALESCE(m.taxes_flat_rate, 0) AS tax
            FROM plutus.symbols s
            JOIN LATERAL (
                SELECT
                    SUM(t.price * t.volume) AS total_spent,
                    MIN(t.price)            AS min_price,
                    MAX(t.price)            AS max_price,
                    SUM(t.volume)           AS total_volume,
                    COUNT(*)                AS num_transactions
                FROM plutus.trades t
                WHERE t.symbol_id = s.id
                  AND t.timestamp >= now() - INTERVAL '1 hour'
                HAVING COUNT(*) > 0
            ) agg ON TRUE
            JOIN plutus.markets  m ON m.id = s.market_id

            UNION ALL

            SELECT
                s.market_id                       AS market_id,
                s.id                              AS symbol_id,
                'FourHours'                       AS time_frame,
                agg.total_spent                   AS total_spent,
                agg.min_price                     AS min_price,
                agg.max_price                     AS max_price,
                agg.total_volume                  AS total_volume,
                agg.num_transactions              AS num_transactions,
                COALESCE(s.additional_fields_limit, agg.total_volume) AS "limit",
                agg.max_price * COALESCE(m.taxes_flat_rate, 0) AS tax
            FROM plutus.symbols s
            JOIN LATERAL (
                SELECT
                    SUM(t.price * t.volume) AS total_spent,
                    MIN(t.price)            AS min_price,
                    MAX(t.price)            AS max_price,
                    SUM(t.volume)           AS total_volume,
                    COUNT(*)                AS num_transactions
                FROM plutus.trades t
                WHERE t.symbol_id = s.id
                  AND t.timestamp >= now() - INTERVAL '4 hours'
                HAVING COUNT(*) > 0
            ) agg ON TRUE
            JOIN plutus.markets  m ON m.id = s.market_id

            UNION ALL

            SELECT
                s.market_id                       AS market_id,
                s.id                              AS symbol_id,
                'OneDay'                       AS time_frame,
                agg.total_spent                   AS total_spent,
                agg.min_price                     AS min_price,
                agg.max_price                     AS max_price,
                agg.total_volume                  AS total_volume,
                agg.num_transactions              AS num_transactions,
                COALESCE(s.additional_fields_limit, agg.total_volume) AS "limit",
                agg.max_price * COALESCE(m.taxes_flat_rate, 0) AS tax
            FROM plutus.symbols s
            JOIN LATERAL (
                SELECT
                    SUM(t.price * t.volume) AS total_spent,
                    MIN(t.price)            AS min_price,
                    MAX(t.price)            AS max_price,
                    SUM(t.volume)           AS total_volume,
                    COUNT(*)                AS num_transactions
                FROM plutus.trades t
                WHERE t.symbol_id = s.id
                  AND t.timestamp >= now() - INTERVAL '1 day'
                HAVING COUNT(*) > 0
            ) agg ON TRUE
            JOIN plutus.markets  m ON m.id = s.market_id;
            """
        );
    }
}

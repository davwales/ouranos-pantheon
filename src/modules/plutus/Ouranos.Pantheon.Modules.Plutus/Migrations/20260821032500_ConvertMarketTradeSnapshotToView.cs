using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Migrations;

/// <inheritdoc />
public partial class ConvertMarketTradeSnapshotToView : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DROP TABLE IF EXISTS plutus.market_trade_snapshots CASCADE;

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

        migrationBuilder.Sql(
            """
            CREATE INDEX IF NOT EXISTS ix_trades_daily_symbol_id_bucket
                ON plutus.trades_daily (symbol_id, bucket);
            """
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DROP VIEW IF EXISTS plutus.market_trade_snapshots CASCADE;

            CREATE TABLE plutus.market_trade_snapshots (
                id               uuid            NOT NULL,
                market_id        uuid            NOT NULL,
                symbol_id        uuid            NOT NULL,
                time_frame       text            NOT NULL,
                total_spent      numeric(18,2)   NOT NULL,
                min_price        numeric(18,2)   NOT NULL,
                max_price        numeric(18,2)   NOT NULL,
                total_volume     numeric(18,2)   NOT NULL,
                num_transactions integer         NOT NULL,
                "limit"          numeric(18,2)   NOT NULL,
                tax              numeric(18,2)   NOT NULL,
                created_at       timestamp with time zone NOT NULL,
                updated_at       timestamp with time zone NOT NULL,
                CONSTRAINT pk_market_trade_snapshots PRIMARY KEY (id),
                CONSTRAINT fk_market_trade_snapshots_markets_market_id
                    FOREIGN KEY (market_id) REFERENCES plutus.markets (id) ON DELETE CASCADE,
                CONSTRAINT fk_market_trade_snapshots_symbols_symbol_id
                    FOREIGN KEY (symbol_id) REFERENCES plutus.symbols (id) ON DELETE CASCADE
            );

            CREATE INDEX ix_market_trade_snapshots_market_id_time_frame
                ON plutus.market_trade_snapshots (market_id, time_frame);

            CREATE UNIQUE INDEX ix_market_trade_snapshots_symbol_id_time_frame
                ON plutus.market_trade_snapshots (symbol_id, time_frame);
            """
        );

        migrationBuilder.Sql(
            """
            DROP INDEX IF EXISTS plutus.ix_trades_daily_symbol_id_bucket;
            """
        );
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Migrations;

/// <inheritdoc />
public partial class ConvertMarketOverviewBucketToView : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            CREATE OR REPLACE FUNCTION plutus.smart_interval(p_duration interval, p_num_buckets int)
            RETURNS interval
            LANGUAGE plpgsql
            IMMUTABLE
            AS $$
            DECLARE
                dur_seconds double precision;
                target double precision;
            BEGIN
                IF p_duration IS NULL OR EXTRACT(EPOCH FROM p_duration) <= 0 THEN
                    RETURN INTERVAL '5 minutes';
                END IF;

                dur_seconds := EXTRACT(EPOCH FROM p_duration);
                target := dur_seconds / p_num_buckets;

                IF target <= 60 THEN
                    RETURN GREATEST(10, ROUND(target)) * INTERVAL '1 second';
                ELSIF target <= 3600 THEN
                    RETURN GREATEST(1, FLOOR(target / 60)) * INTERVAL '1 minute';
                ELSIF target <= 86400 THEN
                    RETURN GREATEST(5, FLOOR(target / 3600) * 60) * INTERVAL '1 minute';
                ELSIF target <= 604800 THEN
                    RETURN GREATEST(1, FLOOR(target / 3600)) * INTERVAL '1 hour';
                ELSIF target <= 2592000 THEN
                    RETURN GREATEST(6, FLOOR(target / 86400) * 24) * INTERVAL '1 hour';
                ELSIF target <= 31536000 THEN
                    RETURN GREATEST(1, FLOOR(target / 86400)) * INTERVAL '1 day';
                ELSE
                    RETURN GREATEST(7, FLOOR(target / 604800) * 7) * INTERVAL '1 day';
                END IF;
            END;
            $$;
            """
        );

        // Market-level daily tier over raw trades + symbols, so the long-timeframe
        // view branches aggregate ~1 row per market per day instead of ~2.2M
        // per-symbol rows. The policy materializes the 2-year window progressively
        // in the background (a full historical CALL is a post-deploy step if needed).
        migrationBuilder.Sql(
            """
            CREATE MATERIALIZED VIEW plutus.trades_market_daily
            WITH (timescaledb.continuous, timescaledb.materialized_only = false) AS
            SELECT
                time_bucket(INTERVAL '1 day', t."timestamp") AS day,
                s.market_id,
                SUM(t.price * t.volume) AS total_spent,
                SUM(t.volume) AS volume,
                COUNT(*) AS num_transactions
            FROM plutus.trades t
            JOIN plutus.symbols s ON s.id = t.symbol_id
            GROUP BY day, s.market_id
            WITH NO DATA;

            SELECT add_continuous_aggregate_policy('plutus.trades_market_daily',
                start_offset => INTERVAL '2 years',
                end_offset => INTERVAL '1 hour',
                schedule_interval => INTERVAL '1 hour',
                if_not_exists => TRUE);
            """
        );

        migrationBuilder.Sql(
            """
            DROP TABLE IF EXISTS plutus.market_overview_buckets CASCADE;

            CREATE OR REPLACE VIEW plutus.market_overview_buckets AS
            SELECT
                m.id AS market_id,
                'OneWeek' AS time_frame,
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
                WHERE d2.market_id = m.id
                  AND d2.day >= now() - INTERVAL '7 days'
                HAVING COUNT(*) > 0 AND MAX(d2.day) > MIN(d2.day)
            ) bounds ON TRUE
            JOIN plutus.trades_market_daily d ON d.market_id = m.id
                AND d.day >= now() - INTERVAL '7 days'
            GROUP BY m.id, bounds.bucket_interval, time_bucket(bounds.bucket_interval, d.day)

            UNION ALL

            SELECT
                m.id AS market_id,
                'OneMonth' AS time_frame,
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
                WHERE d2.market_id = m.id
                  AND d2.day >= now() - INTERVAL '30 days'
                HAVING COUNT(*) > 0 AND MAX(d2.day) > MIN(d2.day)
            ) bounds ON TRUE
            JOIN plutus.trades_market_daily d ON d.market_id = m.id
                AND d.day >= now() - INTERVAL '30 days'
            GROUP BY m.id, bounds.bucket_interval, time_bucket(bounds.bucket_interval, d.day)

            UNION ALL

            SELECT
                m.id AS market_id,
                'SixMonths' AS time_frame,
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
                WHERE d2.market_id = m.id
                  AND d2.day >= now() - INTERVAL '182 days'
                HAVING COUNT(*) > 0 AND MAX(d2.day) > MIN(d2.day)
            ) bounds ON TRUE
            JOIN plutus.trades_market_daily d ON d.market_id = m.id
                AND d.day >= now() - INTERVAL '182 days'
            GROUP BY m.id, bounds.bucket_interval, time_bucket(bounds.bucket_interval, d.day)

            UNION ALL

            SELECT
                m.id AS market_id,
                'OneYear' AS time_frame,
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
                WHERE d2.market_id = m.id
                  AND d2.day >= now() - INTERVAL '365 days'
                HAVING COUNT(*) > 0 AND MAX(d2.day) > MIN(d2.day)
            ) bounds ON TRUE
            JOIN plutus.trades_market_daily d ON d.market_id = m.id
                AND d.day >= now() - INTERVAL '365 days'
            GROUP BY m.id, bounds.bucket_interval, time_bucket(bounds.bucket_interval, d.day)

            UNION ALL

            SELECT
                m.id AS market_id,
                'AllTime' AS time_frame,
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
                WHERE d2.market_id = m.id
                HAVING COUNT(*) > 0 AND MAX(d2.day) > MIN(d2.day)
            ) bounds ON TRUE
            JOIN plutus.trades_market_daily d ON d.market_id = m.id
            GROUP BY m.id, bounds.bucket_interval, time_bucket(bounds.bucket_interval, d.day)

            UNION ALL

            SELECT
                m.id AS market_id,
                'FifteenMinutes' AS time_frame,
                time_bucket(bounds.bucket_interval, t."timestamp") AS bucket_start,
                SUM(t.price * t.volume) AS total_spent,
                SUM(t.volume) AS volume,
                COUNT(*) AS num_transactions,
                SUM(t.price * t.volume) / NULLIF(SUM(t.volume), 0) AS average_price
            FROM plutus.markets m
            JOIN LATERAL (
                SELECT plutus.smart_interval(MAX(t2."timestamp") - MIN(t2."timestamp"), 100) AS bucket_interval
                FROM plutus.trades t2
                JOIN plutus.symbols s2 ON s2.id = t2.symbol_id
                WHERE s2.market_id = m.id
                  AND t2."timestamp" >= now() - INTERVAL '15 minutes'
                HAVING COUNT(*) > 0 AND MAX(t2."timestamp") > MIN(t2."timestamp")
            ) bounds ON TRUE
            JOIN plutus.symbols s ON s.market_id = m.id
            JOIN plutus.trades t ON t.symbol_id = s.id
                AND t."timestamp" >= now() - INTERVAL '15 minutes'
            GROUP BY m.id, bounds.bucket_interval, time_bucket(bounds.bucket_interval, t."timestamp")

            UNION ALL

            SELECT
                m.id AS market_id,
                'OneHour' AS time_frame,
                time_bucket(bounds.bucket_interval, t."timestamp") AS bucket_start,
                SUM(t.price * t.volume) AS total_spent,
                SUM(t.volume) AS volume,
                COUNT(*) AS num_transactions,
                SUM(t.price * t.volume) / NULLIF(SUM(t.volume), 0) AS average_price
            FROM plutus.markets m
            JOIN LATERAL (
                SELECT plutus.smart_interval(MAX(t2."timestamp") - MIN(t2."timestamp"), 100) AS bucket_interval
                FROM plutus.trades t2
                JOIN plutus.symbols s2 ON s2.id = t2.symbol_id
                WHERE s2.market_id = m.id
                  AND t2."timestamp" >= now() - INTERVAL '1 hour'
                HAVING COUNT(*) > 0 AND MAX(t2."timestamp") > MIN(t2."timestamp")
            ) bounds ON TRUE
            JOIN plutus.symbols s ON s.market_id = m.id
            JOIN plutus.trades t ON t.symbol_id = s.id
                AND t."timestamp" >= now() - INTERVAL '1 hour'
            GROUP BY m.id, bounds.bucket_interval, time_bucket(bounds.bucket_interval, t."timestamp")

            UNION ALL

            SELECT
                m.id AS market_id,
                'FourHours' AS time_frame,
                time_bucket(bounds.bucket_interval, t."timestamp") AS bucket_start,
                SUM(t.price * t.volume) AS total_spent,
                SUM(t.volume) AS volume,
                COUNT(*) AS num_transactions,
                SUM(t.price * t.volume) / NULLIF(SUM(t.volume), 0) AS average_price
            FROM plutus.markets m
            JOIN LATERAL (
                SELECT plutus.smart_interval(MAX(t2."timestamp") - MIN(t2."timestamp"), 100) AS bucket_interval
                FROM plutus.trades t2
                JOIN plutus.symbols s2 ON s2.id = t2.symbol_id
                WHERE s2.market_id = m.id
                  AND t2."timestamp" >= now() - INTERVAL '4 hours'
                HAVING COUNT(*) > 0 AND MAX(t2."timestamp") > MIN(t2."timestamp")
            ) bounds ON TRUE
            JOIN plutus.symbols s ON s.market_id = m.id
            JOIN plutus.trades t ON t.symbol_id = s.id
                AND t."timestamp" >= now() - INTERVAL '4 hours'
            GROUP BY m.id, bounds.bucket_interval, time_bucket(bounds.bucket_interval, t."timestamp")

            UNION ALL

            SELECT
                m.id AS market_id,
                'OneDay' AS time_frame,
                time_bucket(bounds.bucket_interval, t."timestamp") AS bucket_start,
                SUM(t.price * t.volume) AS total_spent,
                SUM(t.volume) AS volume,
                COUNT(*) AS num_transactions,
                SUM(t.price * t.volume) / NULLIF(SUM(t.volume), 0) AS average_price
            FROM plutus.markets m
            JOIN LATERAL (
                SELECT plutus.smart_interval(MAX(t2."timestamp") - MIN(t2."timestamp"), 100) AS bucket_interval
                FROM plutus.trades t2
                JOIN plutus.symbols s2 ON s2.id = t2.symbol_id
                WHERE s2.market_id = m.id
                  AND t2."timestamp" >= now() - INTERVAL '1 day'
                HAVING COUNT(*) > 0 AND MAX(t2."timestamp") > MIN(t2."timestamp")
            ) bounds ON TRUE
            JOIN plutus.symbols s ON s.market_id = m.id
            JOIN plutus.trades t ON t.symbol_id = s.id
                AND t."timestamp" >= now() - INTERVAL '1 day'
            GROUP BY m.id, bounds.bucket_interval, time_bucket(bounds.bucket_interval, t."timestamp");
            """
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DROP VIEW IF EXISTS plutus.market_overview_buckets CASCADE;

            DROP MATERIALIZED VIEW IF EXISTS plutus.trades_market_daily CASCADE;

            DROP FUNCTION IF EXISTS plutus.smart_interval(interval, integer) CASCADE;

            CREATE TABLE plutus.market_overview_buckets (
                id               uuid            NOT NULL,
                market_id        uuid            NOT NULL,
                time_frame       text            NOT NULL,
                bucket_start     timestamp with time zone NOT NULL,
                average_price    numeric(18,2)   NOT NULL,
                volume           numeric(18,2)   NOT NULL,
                total_spent      numeric(18,2)   NOT NULL,
                num_transactions integer         NOT NULL,
                created_at       timestamp with time zone NOT NULL,
                updated_at       timestamp with time zone NOT NULL,
                CONSTRAINT pk_market_overview_buckets PRIMARY KEY (id),
                CONSTRAINT fk_market_overview_buckets_markets_market_id
                    FOREIGN KEY (market_id) REFERENCES plutus.markets (id) ON DELETE CASCADE
            );

            CREATE INDEX ix_market_overview_buckets_market_id_time_frame
                ON plutus.market_overview_buckets (market_id, time_frame);
            """
        );
    }
}

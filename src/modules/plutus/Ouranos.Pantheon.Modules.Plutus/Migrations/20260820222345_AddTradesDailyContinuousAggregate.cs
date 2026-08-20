using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Migrations;

/// <inheritdoc />
public partial class AddTradesDailyContinuousAggregate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            CREATE MATERIALIZED VIEW plutus.trades_daily
            WITH (timescaledb.continuous, timescaledb.materialized_only = false) AS
            SELECT
                time_bucket(INTERVAL '1 day', timestamp) AS bucket,
                symbol_id,
                SUM(price * volume) AS total_spent,
                MIN(price)             AS min_price,
                MAX(price)             AS max_price,
                SUM(volume)            AS volume,
                COUNT(*)               AS num_transactions
            FROM plutus.trades
            GROUP BY bucket, symbol_id
            WITH NO DATA;
            """
        );

        migrationBuilder.Sql(
            """
            SELECT add_continuous_aggregate_policy('plutus.trades_daily',
                start_offset => INTERVAL '2 years',
                end_offset => INTERVAL '1 hour',
                schedule_interval => INTERVAL '1 hour',
                if_not_exists => TRUE);
            """
        );

        // The one-time full backfill
        // (CALL refresh_continuous_aggregate('plutus.trades_daily', NULL, now() - INTERVAL '1 hour'))
        // is a post-deploy step: CALL cannot run inside a migration transaction, and a full historical
        // materialization would block Database.MigrateAsync() at startup. materialized_only = false
        // keeps the aggregate correct for reads while the materialized history is incomplete.
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DROP MATERIALIZED VIEW IF EXISTS plutus.trades_daily CASCADE;
            """
        );
    }
}

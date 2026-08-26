using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Migrations;

/// <inheritdoc />
public partial class ConvertSignalsToHypertable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE plutus.signals DROP CONSTRAINT IF EXISTS pk_signals;
            ALTER TABLE plutus.signals DROP CONSTRAINT IF EXISTS signals_pkey;
            ALTER TABLE plutus.signals ADD CONSTRAINT pk_signals PRIMARY KEY (id, computed_at);
            """
        );

        migrationBuilder.Sql(
            """
            SELECT create_hypertable('plutus.signals', 'computed_at',
                chunk_time_interval => INTERVAL '1 day',
                migrate_data => TRUE,
                if_not_exists => TRUE);

            SELECT add_retention_policy('plutus.signals',
                drop_after => INTERVAL '7 days',
                schedule_interval => INTERVAL '1 day',
                if_not_exists => TRUE);
            """
        );

        migrationBuilder.Sql(
            """
            CREATE MATERIALIZED VIEW plutus.signal_history_30m
            WITH (timescaledb.continuous, timescaledb.materialized_only = false) AS
            SELECT
                symbol_id,
                type AS signal_type,
                time_bucket(INTERVAL '30 minutes', computed_at) AS bucket,
                LAST(value, computed_at) AS last_value,
                AVG(value) AS avg_value,
                MIN(value) AS min_value,
                MAX(value) AS max_value,
                COUNT(*) AS sample_count
            FROM plutus.signals
            GROUP BY symbol_id, type, bucket
            WITH NO DATA;

            SELECT add_continuous_aggregate_policy('plutus.signal_history_30m',
                start_offset => INTERVAL '7 days',
                end_offset => INTERVAL '30 minutes',
                schedule_interval => INTERVAL '5 minutes',
                if_not_exists => TRUE);
            """
        );

        migrationBuilder.Sql(
            """
            ALTER TABLE plutus.signals SET (
                timescaledb.compress,
                timescaledb.compress_segmentby = 'symbol_id'
            );
            """
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DROP MATERIALIZED VIEW IF EXISTS plutus.signal_history_30m CASCADE;
            """
        );

        migrationBuilder.Sql(
            """
            SELECT remove_retention_policy('plutus.signals', if_exists => TRUE);
            """
        );

        migrationBuilder.Sql(
            """
            ALTER TABLE plutus.signals SET (timescaledb.compress = false);
            """
        );

        // The hypertable conversion itself (create_hypertable with migrate_data)
        // cannot be automatically reversed. TimescaleDB does not provide a built-in
        // way to turn a hypertable back into a regular table, and any unique/PK
        // index that excludes the partitioning column (computed_at) is rejected,
        // so the original single-column PK on `id` cannot be restored while the
        // table remains a hypertable.
        //
        // A full reversal would require: creating a new regular table with the
        // original schema, copying the data across, dropping the hypertable, and
        // renaming the new table into place. That is a destructive, data-moving
        // operation that is intentionally left to operators - if a full rollback
        // is required, restore from a backup taken before this migration.
        //
        // Down is therefore a safe no-op for the hypertable conversion itself:
        // the continuous aggregate view and the retention/compression policies
        // above are torn down, but the hypertable structure is preserved.
    }
}

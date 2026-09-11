using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Migrations;

/// <inheritdoc />
public partial class ConvertLatestSignalsToMaterializedView : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DROP VIEW IF EXISTS plutus.latest_signals;

            CREATE MATERIALIZED VIEW plutus.latest_signals AS
            SELECT DISTINCT ON (symbol_id, signal_type)
                symbol_id, signal_type, last_value
            FROM plutus.signal_history_30m
            WHERE bucket >= now() - INTERVAL '2 hours'
            ORDER BY symbol_id, signal_type, bucket DESC;

            CREATE UNIQUE INDEX ix_latest_signals_symbol_id_signal_type
                ON plutus.latest_signals (symbol_id, signal_type);
            """
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DROP MATERIALIZED VIEW IF EXISTS plutus.latest_signals;

            CREATE OR REPLACE VIEW plutus.latest_signals AS
            SELECT DISTINCT ON (symbol_id, signal_type)
                symbol_id, signal_type, last_value
            FROM plutus.signal_history_30m
            WHERE bucket >= now() - INTERVAL '2 hours'
            ORDER BY symbol_id, signal_type, bucket DESC;
            """
        );
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Migrations;

/// <inheritdoc />
public partial class AddUniqueConstraintsToInputWeightTables : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Remove duplicate input-weight rows (multiple entries for the same kind)
        // before enforcing uniqueness. Keeps the row with the highest id per
        // (owner, kind). Must compare within the group: legacy ids are not globally
        // unique (the backfill used per-owner ROW_NUMBER), so a plain
        // `id NOT IN (SELECT MAX(id)...)` would wrongly keep duplicates.
        migrationBuilder.Sql(
            """
            DELETE FROM plutus.backtests_optimized_input_weights a
            USING plutus.backtests_optimized_input_weights b
            WHERE a.backtest_results_backtest_id = b.backtest_results_backtest_id
              AND a.kind = b.kind
              AND a.id < b.id;
            """
        );

        migrationBuilder.Sql(
            """
            DELETE FROM plutus.strategies_input_weights a
            USING plutus.strategies_input_weights b
            WHERE a.strategy_id = b.strategy_id
              AND a.kind = b.kind
              AND a.id < b.id;
            """
        );

        migrationBuilder.CreateIndex(
            name: "ix_strategies_input_weights_strategy_id_kind",
            schema: "plutus",
            table: "strategies_input_weights",
            columns: new[] { "strategy_id", "kind" },
            unique: true
        );

        migrationBuilder.CreateIndex(
            name: "ix_backtests_optimized_input_weights_backtest_results_backtest",
            schema: "plutus",
            table: "backtests_optimized_input_weights",
            columns: ["backtest_results_backtest_id", "kind"],
            unique: true
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_strategies_input_weights_strategy_id_kind",
            schema: "plutus",
            table: "strategies_input_weights"
        );

        migrationBuilder.DropIndex(
            name: "ix_backtests_optimized_input_weights_backtest_results_backtest",
            schema: "plutus",
            table: "backtests_optimized_input_weights"
        );
    }
}

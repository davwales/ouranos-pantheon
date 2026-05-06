using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Migrations;

/// <inheritdoc />
public partial class AddOptimizedConfigurationToBacktestResults : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "results_optimized_configuration_buy_threshold",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );

        migrationBuilder.AddColumn<decimal>(
            name: "results_optimized_configuration_deviation_multiplier",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );

        migrationBuilder.AddColumn<int>(
            name: "results_optimized_configuration_forecast_horizon_days",
            schema: "plutus",
            table: "backtests",
            type: "integer",
            nullable: true
        );

        migrationBuilder.AddColumn<decimal>(
            name: "results_optimized_configuration_forecast_movement_threshold",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );

        migrationBuilder.AddColumn<int>(
            name: "results_optimized_configuration_hold_period_days",
            schema: "plutus",
            table: "backtests",
            type: "integer",
            nullable: true
        );

        migrationBuilder.AddColumn<decimal>(
            name: "results_optimized_configuration_max_position_percent",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );

        migrationBuilder.AddColumn<int>(
            name: "results_optimized_configuration_max_positions",
            schema: "plutus",
            table: "backtests",
            type: "integer",
            nullable: true
        );

        migrationBuilder.AddColumn<int>(
            name: "results_optimized_configuration_mean_time_frame_value",
            schema: "plutus",
            table: "backtests",
            type: "integer",
            nullable: true
        );

        migrationBuilder.AddColumn<decimal>(
            name: "results_optimized_configuration_min_margin_percent",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );

        migrationBuilder.AddColumn<decimal>(
            name: "results_optimized_configuration_sell_threshold",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );

        migrationBuilder.CreateTable(
            name: "backtest_optimized_components",
            schema: "plutus",
            columns: table => new
            {
                strategy_configuration_backtest_results_backtest_id = table.Column<Guid>(type: "uuid", nullable: false),
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation(
                        "Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                    ),
                strategy_id = table.Column<Guid>(type: "uuid", nullable: false),
                type = table.Column<int>(type: "integer", nullable: false),
                weight = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "pk_backtest_optimized_components",
                    x => new { x.strategy_configuration_backtest_results_backtest_id, x.id }
                );
                table.ForeignKey(
                    name: "fk_backtest_optimized_components_backtests_strategy_configurat",
                    column: x => x.strategy_configuration_backtest_results_backtest_id,
                    principalSchema: "plutus",
                    principalTable: "backtests",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateTable(
            name: "backtest_optimized_signal_weights",
            schema: "plutus",
            columns: table => new
            {
                strategy_configuration_backtest_results_backtest_id = table.Column<Guid>(type: "uuid", nullable: false),
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation(
                        "Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                    ),
                type = table.Column<int>(type: "integer", nullable: false),
                weight = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "pk_backtest_optimized_signal_weights",
                    x => new { x.strategy_configuration_backtest_results_backtest_id, x.id }
                );
                table.ForeignKey(
                    name: "fk_backtest_optimized_signal_weights_backtests_strategy_config",
                    column: x => x.strategy_configuration_backtest_results_backtest_id,
                    principalSchema: "plutus",
                    principalTable: "backtests",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "backtest_optimized_components",
            schema: "plutus"
        );

        migrationBuilder.DropTable(
            name: "backtest_optimized_signal_weights",
            schema: "plutus"
        );

        migrationBuilder.DropColumn(
            name: "results_optimized_configuration_buy_threshold",
            schema: "plutus",
            table: "backtests"
        );

        migrationBuilder.DropColumn(
            name: "results_optimized_configuration_deviation_multiplier",
            schema: "plutus",
            table: "backtests"
        );

        migrationBuilder.DropColumn(
            name: "results_optimized_configuration_forecast_horizon_days",
            schema: "plutus",
            table: "backtests"
        );

        migrationBuilder.DropColumn(
            name: "results_optimized_configuration_forecast_movement_threshold",
            schema: "plutus",
            table: "backtests"
        );

        migrationBuilder.DropColumn(
            name: "results_optimized_configuration_hold_period_days",
            schema: "plutus",
            table: "backtests"
        );

        migrationBuilder.DropColumn(
            name: "results_optimized_configuration_max_position_percent",
            schema: "plutus",
            table: "backtests"
        );

        migrationBuilder.DropColumn(
            name: "results_optimized_configuration_max_positions",
            schema: "plutus",
            table: "backtests"
        );

        migrationBuilder.DropColumn(
            name: "results_optimized_configuration_mean_time_frame_value",
            schema: "plutus",
            table: "backtests"
        );

        migrationBuilder.DropColumn(
            name: "results_optimized_configuration_min_margin_percent",
            schema: "plutus",
            table: "backtests"
        );

        migrationBuilder.DropColumn(
            name: "results_optimized_configuration_sell_threshold",
            schema: "plutus",
            table: "backtests"
        );
    }
}

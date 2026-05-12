using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Migrations;

/// <inheritdoc />
public partial class DecomposeStrategyConfiguration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_backtest_position_backtests_backtest_results_backtest_id",
            schema: "plutus",
            table: "backtest_position"
        );

        migrationBuilder.DropForeignKey(
            name: "fk_composite_component_strategies_strategy_configuration_strat",
            schema: "plutus",
            table: "composite_component"
        );

        migrationBuilder.RenameColumn(
            name: "configuration_sell_threshold",
            schema: "plutus",
            table: "strategies",
            newName: "signal_weighted_config_sell_threshold"
        );

        migrationBuilder.RenameColumn(
            name: "configuration_min_margin_percent",
            schema: "plutus",
            table: "strategies",
            newName: "recipe_arbitrage_config_min_margin_percent"
        );

        migrationBuilder.RenameColumn(
            name: "configuration_mean_time_frame_value",
            schema: "plutus",
            table: "strategies",
            newName: "mean_reversion_config_mean_time_frame_value"
        );

        migrationBuilder.RenameColumn(
            name: "configuration_max_positions",
            schema: "plutus",
            table: "strategies",
            newName: "trading_configuration_max_positions"
        );

        migrationBuilder.RenameColumn(
            name: "configuration_max_position_percent",
            schema: "plutus",
            table: "strategies",
            newName: "trading_configuration_max_position_percent"
        );

        migrationBuilder.RenameColumn(
            name: "configuration_hold_period_days",
            schema: "plutus",
            table: "strategies",
            newName: "trading_configuration_hold_period_days"
        );

        migrationBuilder.RenameColumn(
            name: "configuration_forecast_movement_threshold",
            schema: "plutus",
            table: "strategies",
            newName: "forecast_momentum_config_forecast_movement_threshold"
        );

        migrationBuilder.RenameColumn(
            name: "configuration_forecast_horizon_days",
            schema: "plutus",
            table: "strategies",
            newName: "forecast_momentum_config_forecast_horizon_days"
        );

        migrationBuilder.RenameColumn(
            name: "configuration_deviation_multiplier",
            schema: "plutus",
            table: "strategies",
            newName: "mean_reversion_config_deviation_multiplier"
        );

        migrationBuilder.RenameColumn(
            name: "configuration_buy_threshold",
            schema: "plutus",
            table: "strategies",
            newName: "signal_weighted_config_buy_threshold"
        );

        migrationBuilder.RenameColumn(
            name: "results_optimized_configuration_sell_threshold",
            schema: "plutus",
            table: "backtests",
            newName: "results_optimized_signal_weighted_config_sell_threshold"
        );

        migrationBuilder.RenameColumn(
            name: "results_optimized_configuration_min_margin_percent",
            schema: "plutus",
            table: "backtests",
            newName: "results_optimized_recipe_arbitrage_config_min_margin_percent"
        );

        migrationBuilder.RenameColumn(
            name: "results_optimized_configuration_mean_time_frame_value",
            schema: "plutus",
            table: "backtests",
            newName: "results_optimized_mean_reversion_config_mean_time_frame_value"
        );

        migrationBuilder.RenameColumn(
            name: "results_optimized_configuration_forecast_movement_threshold",
            schema: "plutus",
            table: "backtests",
            newName: "results_optimized_forecast_momentum_config_forecast_movement_thresh"
        );

        migrationBuilder.RenameColumn(
            name: "results_optimized_configuration_forecast_horizon_days",
            schema: "plutus",
            table: "backtests",
            newName: "results_optimized_forecast_momentum_config_forecast_horizon_days"
        );

        migrationBuilder.RenameColumn(
            name: "results_optimized_configuration_deviation_multiplier",
            schema: "plutus",
            table: "backtests",
            newName: "results_optimized_mean_reversion_config_deviation_multiplier"
        );

        migrationBuilder.RenameColumn(
            name: "results_optimized_configuration_buy_threshold",
            schema: "plutus",
            table: "backtests",
            newName: "results_optimized_signal_weighted_config_buy_threshold"
        );

        migrationBuilder.RenameColumn(
            name: "backtest_results_backtest_id",
            schema: "plutus",
            table: "backtest_position",
            newName: "backtest_id"
        );

        migrationBuilder.AddColumn<decimal>(
            name: "signal_weighted_config_bollinger_bands_weight",
            schema: "plutus",
            table: "strategies",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );

        migrationBuilder.AddColumn<decimal>(
            name: "signal_weighted_config_moving_average_crossover_weight",
            schema: "plutus",
            table: "strategies",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );

        migrationBuilder.AddColumn<decimal>(
            name: "signal_weighted_config_price_velocity_weight",
            schema: "plutus",
            table: "strategies",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );

        migrationBuilder.AddColumn<decimal>(
            name: "signal_weighted_config_rsi_weight",
            schema: "plutus",
            table: "strategies",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );

        migrationBuilder.AddColumn<decimal>(
            name: "signal_weighted_config_tax_adjusted_roi_weight",
            schema: "plutus",
            table: "strategies",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );

        migrationBuilder.AddColumn<decimal>(
            name: "signal_weighted_config_trend_momentum_weight",
            schema: "plutus",
            table: "strategies",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );

        migrationBuilder.AddColumn<decimal>(
            name: "signal_weighted_config_volume_anomaly_weight",
            schema: "plutus",
            table: "strategies",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );

        migrationBuilder.AddColumn<decimal>(
            name: "results_optimized_signal_weighted_config_bollinger_bands_weight",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );

        migrationBuilder.AddColumn<decimal>(
            name: "results_optimized_signal_weighted_config_moving_average_crossover_we",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );

        migrationBuilder.AddColumn<decimal>(
            name: "results_optimized_signal_weighted_config_price_velocity_weight",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );

        migrationBuilder.AddColumn<decimal>(
            name: "results_optimized_signal_weighted_config_rsi_weight",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );

        migrationBuilder.AddColumn<decimal>(
            name: "results_optimized_signal_weighted_config_tax_adjusted_roi_weight",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );

        migrationBuilder.AddColumn<decimal>(
            name: "results_optimized_signal_weighted_config_trend_momentum_weight",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );

        migrationBuilder.AddColumn<decimal>(
            name: "results_optimized_signal_weighted_config_volume_anomaly_weight",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );

        // Migrate data from signal_weight table to strategies columns
        migrationBuilder.Sql(
            """
            UPDATE plutus.strategies s
            SET
                signal_weighted_config_tax_adjusted_roi_weight = sw.weight
            FROM plutus.signal_weight sw
            WHERE sw.strategy_configuration_strategy_id = s.id AND sw.type = 1
            """
        );

        migrationBuilder.Sql(
            """
            UPDATE plutus.strategies s
            SET
                signal_weighted_config_volume_anomaly_weight = sw.weight
            FROM plutus.signal_weight sw
            WHERE sw.strategy_configuration_strategy_id = s.id AND sw.type = 2
            """
        );

        migrationBuilder.Sql(
            """
            UPDATE plutus.strategies s
            SET
                signal_weighted_config_trend_momentum_weight = sw.weight
            FROM plutus.signal_weight sw
            WHERE sw.strategy_configuration_strategy_id = s.id AND sw.type = 3
            """
        );

        migrationBuilder.Sql(
            """
            UPDATE plutus.strategies s
            SET
                signal_weighted_config_bollinger_bands_weight = sw.weight
            FROM plutus.signal_weight sw
            WHERE sw.strategy_configuration_strategy_id = s.id AND sw.type = 4
            """
        );

        migrationBuilder.Sql(
            """
            UPDATE plutus.strategies s
            SET
                signal_weighted_config_rsi_weight = sw.weight
            FROM plutus.signal_weight sw
            WHERE sw.strategy_configuration_strategy_id = s.id AND sw.type = 5
            """
        );

        migrationBuilder.Sql(
            """
            UPDATE plutus.strategies s
            SET
                signal_weighted_config_moving_average_crossover_weight = sw.weight
            FROM plutus.signal_weight sw
            WHERE sw.strategy_configuration_strategy_id = s.id AND sw.type = 6
            """
        );

        migrationBuilder.Sql(
            """
            UPDATE plutus.strategies s
            SET
                signal_weighted_config_price_velocity_weight = sw.weight
            FROM plutus.signal_weight sw
            WHERE sw.strategy_configuration_strategy_id = s.id AND sw.type = 7
            """
        );

        // Migrate data from backtest_optimized_signal_weights to backtests columns
        migrationBuilder.Sql(
            """
            UPDATE plutus.backtests b
            SET
                results_optimized_signal_weighted_config_tax_adjusted_roi_weight = sw.weight
            FROM plutus.backtest_optimized_signal_weights sw
            WHERE sw.strategy_configuration_backtest_results_backtest_id = b.id AND sw.type = 1
            """
        );

        migrationBuilder.Sql(
            """
            UPDATE plutus.backtests b
            SET
                results_optimized_signal_weighted_config_volume_anomaly_weight = sw.weight
            FROM plutus.backtest_optimized_signal_weights sw
            WHERE sw.strategy_configuration_backtest_results_backtest_id = b.id AND sw.type = 2
            """
        );

        migrationBuilder.Sql(
            """
            UPDATE plutus.backtests b
            SET
                results_optimized_signal_weighted_config_trend_momentum_weight = sw.weight
            FROM plutus.backtest_optimized_signal_weights sw
            WHERE sw.strategy_configuration_backtest_results_backtest_id = b.id AND sw.type = 3
            """
        );

        migrationBuilder.Sql(
            """
            UPDATE plutus.backtests b
            SET
                results_optimized_signal_weighted_config_bollinger_bands_weight = sw.weight
            FROM plutus.backtest_optimized_signal_weights sw
            WHERE sw.strategy_configuration_backtest_results_backtest_id = b.id AND sw.type = 4
            """
        );

        migrationBuilder.Sql(
            """
            UPDATE plutus.backtests b
            SET
                results_optimized_signal_weighted_config_rsi_weight = sw.weight
            FROM plutus.backtest_optimized_signal_weights sw
            WHERE sw.strategy_configuration_backtest_results_backtest_id = b.id AND sw.type = 5
            """
        );

        migrationBuilder.Sql(
            """
            UPDATE plutus.backtests b
            SET
                results_optimized_signal_weighted_config_moving_average_crossover_we = sw.weight
            FROM plutus.backtest_optimized_signal_weights sw
            WHERE sw.strategy_configuration_backtest_results_backtest_id = b.id AND sw.type = 6
            """
        );

        migrationBuilder.Sql(
            """
            UPDATE plutus.backtests b
            SET
                results_optimized_signal_weighted_config_price_velocity_weight = sw.weight
            FROM plutus.backtest_optimized_signal_weights sw
            WHERE sw.strategy_configuration_backtest_results_backtest_id = b.id AND sw.type = 7
            """
        );

        migrationBuilder.DropTable(
            name: "backtest_optimized_components",
            schema: "plutus"
        );

        migrationBuilder.DropTable(
            name: "backtest_optimized_signal_weights",
            schema: "plutus"
        );

        migrationBuilder.DropTable(
            name: "signal_weight",
            schema: "plutus"
        );

        migrationBuilder.DropPrimaryKey(
            name: "pk_composite_component",
            schema: "plutus",
            table: "composite_component"
        );

        migrationBuilder.DropColumn(
            name: "strategy_configuration_strategy_id",
            schema: "plutus",
            table: "composite_component"
        );

        migrationBuilder.AddPrimaryKey(
            name: "pk_composite_component",
            schema: "plutus",
            table: "composite_component",
            columns: new[] { "strategy_id", "id" }
        );

        migrationBuilder.AddForeignKey(
            name: "fk_backtest_position_backtests_backtest_id",
            schema: "plutus",
            table: "backtest_position",
            column: "backtest_id",
            principalSchema: "plutus",
            principalTable: "backtests",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade
        );

        migrationBuilder.AddForeignKey(
            name: "fk_composite_component_strategies_strategy_id",
            schema: "plutus",
            table: "composite_component",
            column: "strategy_id",
            principalSchema: "plutus",
            principalTable: "strategies",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_backtest_position_backtests_backtest_id",
            schema: "plutus",
            table: "backtest_position"
        );

        migrationBuilder.DropForeignKey(
            name: "fk_composite_component_strategies_strategy_id",
            schema: "plutus",
            table: "composite_component"
        );

        migrationBuilder.DropPrimaryKey(
            name: "pk_composite_component",
            schema: "plutus",
            table: "composite_component"
        );

        migrationBuilder.DropColumn(
            name: "signal_weighted_config_bollinger_bands_weight",
            schema: "plutus",
            table: "strategies"
        );

        migrationBuilder.DropColumn(
            name: "signal_weighted_config_moving_average_crossover_weight",
            schema: "plutus",
            table: "strategies"
        );

        migrationBuilder.DropColumn(
            name: "signal_weighted_config_price_velocity_weight",
            schema: "plutus",
            table: "strategies"
        );

        migrationBuilder.DropColumn(
            name: "signal_weighted_config_rsi_weight",
            schema: "plutus",
            table: "strategies"
        );

        migrationBuilder.DropColumn(
            name: "signal_weighted_config_tax_adjusted_roi_weight",
            schema: "plutus",
            table: "strategies"
        );

        migrationBuilder.DropColumn(
            name: "signal_weighted_config_trend_momentum_weight",
            schema: "plutus",
            table: "strategies"
        );

        migrationBuilder.DropColumn(
            name: "signal_weighted_config_volume_anomaly_weight",
            schema: "plutus",
            table: "strategies"
        );

        migrationBuilder.DropColumn(
            name: "results_optimized_signal_weighted_config_bollinger_bands_weight",
            schema: "plutus",
            table: "backtests"
        );

        migrationBuilder.DropColumn(
            name: "results_optimized_signal_weighted_config_moving_average_crossover_we",
            schema: "plutus",
            table: "backtests"
        );

        migrationBuilder.DropColumn(
            name: "results_optimized_signal_weighted_config_price_velocity_weight",
            schema: "plutus",
            table: "backtests"
        );

        migrationBuilder.DropColumn(
            name: "results_optimized_signal_weighted_config_rsi_weight",
            schema: "plutus",
            table: "backtests"
        );

        migrationBuilder.DropColumn(
            name: "results_optimized_signal_weighted_config_tax_adjusted_roi_weight",
            schema: "plutus",
            table: "backtests"
        );

        migrationBuilder.DropColumn(
            name: "results_optimized_signal_weighted_config_trend_momentum_weight",
            schema: "plutus",
            table: "backtests"
        );

        migrationBuilder.DropColumn(
            name: "results_optimized_signal_weighted_config_volume_anomaly_weight",
            schema: "plutus",
            table: "backtests"
        );

        migrationBuilder.RenameColumn(
            name: "trading_configuration_max_positions",
            schema: "plutus",
            table: "strategies",
            newName: "configuration_max_positions"
        );

        migrationBuilder.RenameColumn(
            name: "trading_configuration_max_position_percent",
            schema: "plutus",
            table: "strategies",
            newName: "configuration_max_position_percent"
        );

        migrationBuilder.RenameColumn(
            name: "trading_configuration_hold_period_days",
            schema: "plutus",
            table: "strategies",
            newName: "configuration_hold_period_days"
        );

        migrationBuilder.RenameColumn(
            name: "signal_weighted_config_sell_threshold",
            schema: "plutus",
            table: "strategies",
            newName: "configuration_sell_threshold"
        );

        migrationBuilder.RenameColumn(
            name: "signal_weighted_config_buy_threshold",
            schema: "plutus",
            table: "strategies",
            newName: "configuration_buy_threshold"
        );

        migrationBuilder.RenameColumn(
            name: "recipe_arbitrage_config_min_margin_percent",
            schema: "plutus",
            table: "strategies",
            newName: "configuration_min_margin_percent"
        );

        migrationBuilder.RenameColumn(
            name: "mean_reversion_config_mean_time_frame_value",
            schema: "plutus",
            table: "strategies",
            newName: "configuration_mean_time_frame_value"
        );

        migrationBuilder.RenameColumn(
            name: "mean_reversion_config_deviation_multiplier",
            schema: "plutus",
            table: "strategies",
            newName: "configuration_deviation_multiplier"
        );

        migrationBuilder.RenameColumn(
            name: "forecast_momentum_config_forecast_movement_threshold",
            schema: "plutus",
            table: "strategies",
            newName: "configuration_forecast_movement_threshold"
        );

        migrationBuilder.RenameColumn(
            name: "forecast_momentum_config_forecast_horizon_days",
            schema: "plutus",
            table: "strategies",
            newName: "configuration_forecast_horizon_days"
        );

        migrationBuilder.RenameColumn(
            name: "results_optimized_signal_weighted_config_sell_threshold",
            schema: "plutus",
            table: "backtests",
            newName: "results_optimized_configuration_sell_threshold"
        );

        migrationBuilder.RenameColumn(
            name: "results_optimized_signal_weighted_config_buy_threshold",
            schema: "plutus",
            table: "backtests",
            newName: "results_optimized_configuration_buy_threshold"
        );

        migrationBuilder.RenameColumn(
            name: "results_optimized_recipe_arbitrage_config_min_margin_percent",
            schema: "plutus",
            table: "backtests",
            newName: "results_optimized_configuration_min_margin_percent"
        );

        migrationBuilder.RenameColumn(
            name: "results_optimized_mean_reversion_config_mean_time_frame_value",
            schema: "plutus",
            table: "backtests",
            newName: "results_optimized_configuration_mean_time_frame_value"
        );

        migrationBuilder.RenameColumn(
            name: "results_optimized_mean_reversion_config_deviation_multiplier",
            schema: "plutus",
            table: "backtests",
            newName: "results_optimized_configuration_deviation_multiplier"
        );

        migrationBuilder.RenameColumn(
            name: "results_optimized_forecast_momentum_config_forecast_movement_thresh",
            schema: "plutus",
            table: "backtests",
            newName: "results_optimized_configuration_forecast_movement_threshold"
        );

        migrationBuilder.RenameColumn(
            name: "results_optimized_forecast_momentum_config_forecast_horizon_days",
            schema: "plutus",
            table: "backtests",
            newName: "results_optimized_configuration_forecast_horizon_days"
        );

        migrationBuilder.RenameColumn(
            name: "backtest_id",
            schema: "plutus",
            table: "backtest_position",
            newName: "backtest_results_backtest_id"
        );

        migrationBuilder.AddColumn<Guid>(
            name: "strategy_configuration_strategy_id",
            schema: "plutus",
            table: "composite_component",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
        );

        migrationBuilder.AddPrimaryKey(
            name: "pk_composite_component",
            schema: "plutus",
            table: "composite_component",
            columns: new[] { "strategy_configuration_strategy_id", "id" }
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

        migrationBuilder.CreateTable(
            name: "signal_weight",
            schema: "plutus",
            columns: table => new
            {
                strategy_configuration_strategy_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                table.PrimaryKey("pk_signal_weight", x => new { x.strategy_configuration_strategy_id, x.id });
                table.ForeignKey(
                    name: "fk_signal_weight_strategies_strategy_configuration_strategy_id",
                    column: x => x.strategy_configuration_strategy_id,
                    principalSchema: "plutus",
                    principalTable: "strategies",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.AddForeignKey(
            name: "fk_backtest_position_backtests_backtest_results_backtest_id",
            schema: "plutus",
            table: "backtest_position",
            column: "backtest_results_backtest_id",
            principalSchema: "plutus",
            principalTable: "backtests",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade
        );

        migrationBuilder.AddForeignKey(
            name: "fk_composite_component_strategies_strategy_configuration_strat",
            schema: "plutus",
            table: "composite_component",
            column: "strategy_configuration_strategy_id",
            principalSchema: "plutus",
            principalTable: "strategies",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade
        );
    }
}

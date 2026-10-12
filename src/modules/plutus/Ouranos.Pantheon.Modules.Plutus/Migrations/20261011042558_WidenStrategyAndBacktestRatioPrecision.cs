using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Migrations;

/// <inheritdoc />
public partial class WidenStrategyAndBacktestRatioPrecision : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<decimal>(
            name: "weight",
            schema: "plutus",
            table: "strategies_input_weights",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "trading_configuration_max_position_percent",
            schema: "plutus",
            table: "strategies",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "thresholds_sell_threshold",
            schema: "plutus",
            table: "strategies",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "thresholds_buy_threshold",
            schema: "plutus",
            table: "strategies",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "weight",
            schema: "plutus",
            table: "backtests_optimized_input_weights",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_worst_trade",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_win_rate",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_turnover_rate",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_total_return_percent",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_sortino_ratio",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_sharpe_ratio",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_profit_factor",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_out_sample_results_worst_trade",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_out_sample_results_win_rate",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_out_sample_results_total_return_percent",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_out_sample_results_sortino_ratio",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_out_sample_results_sharpe_ratio",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_out_sample_results_profit_factor",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_out_sample_results_max_drawdown_percent",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_out_sample_results_best_trade",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_out_sample_results_average_trade_return",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_optimized_thresholds_sell_threshold",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_optimized_thresholds_buy_threshold",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_optimized_configuration_max_position_percent",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_max_drawdown_percent",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_best_trade",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_average_trade_return",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "return_percent",
            schema: "plutus",
            table: "backtest_position",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<decimal>(
            name: "weight",
            schema: "plutus",
            table: "strategies_input_weights",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,6)",
            oldPrecision: 18,
            oldScale: 6
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "trading_configuration_max_position_percent",
            schema: "plutus",
            table: "strategies",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,6)",
            oldPrecision: 18,
            oldScale: 6,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "thresholds_sell_threshold",
            schema: "plutus",
            table: "strategies",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,6)",
            oldPrecision: 18,
            oldScale: 6,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "thresholds_buy_threshold",
            schema: "plutus",
            table: "strategies",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,6)",
            oldPrecision: 18,
            oldScale: 6,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "weight",
            schema: "plutus",
            table: "backtests_optimized_input_weights",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,6)",
            oldPrecision: 18,
            oldScale: 6
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_worst_trade",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,6)",
            oldPrecision: 18,
            oldScale: 6,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_win_rate",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,6)",
            oldPrecision: 18,
            oldScale: 6,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_turnover_rate",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,6)",
            oldPrecision: 18,
            oldScale: 6,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_total_return_percent",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,6)",
            oldPrecision: 18,
            oldScale: 6,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_sortino_ratio",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,6)",
            oldPrecision: 18,
            oldScale: 6,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_sharpe_ratio",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,6)",
            oldPrecision: 18,
            oldScale: 6,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_profit_factor",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,6)",
            oldPrecision: 18,
            oldScale: 6,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_out_sample_results_worst_trade",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,6)",
            oldPrecision: 18,
            oldScale: 6,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_out_sample_results_win_rate",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,6)",
            oldPrecision: 18,
            oldScale: 6,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_out_sample_results_total_return_percent",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,6)",
            oldPrecision: 18,
            oldScale: 6,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_out_sample_results_sortino_ratio",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,6)",
            oldPrecision: 18,
            oldScale: 6,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_out_sample_results_sharpe_ratio",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,6)",
            oldPrecision: 18,
            oldScale: 6,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_out_sample_results_profit_factor",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,6)",
            oldPrecision: 18,
            oldScale: 6,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_out_sample_results_max_drawdown_percent",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,6)",
            oldPrecision: 18,
            oldScale: 6,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_out_sample_results_best_trade",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,6)",
            oldPrecision: 18,
            oldScale: 6,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_out_sample_results_average_trade_return",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,6)",
            oldPrecision: 18,
            oldScale: 6,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_optimized_thresholds_sell_threshold",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,6)",
            oldPrecision: 18,
            oldScale: 6,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_optimized_thresholds_buy_threshold",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,6)",
            oldPrecision: 18,
            oldScale: 6,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_optimized_configuration_max_position_percent",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,6)",
            oldPrecision: 18,
            oldScale: 6,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_max_drawdown_percent",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,6)",
            oldPrecision: 18,
            oldScale: 6,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_best_trade",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,6)",
            oldPrecision: 18,
            oldScale: 6,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "results_average_trade_return",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,6)",
            oldPrecision: 18,
            oldScale: 6,
            oldNullable: true
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "return_percent",
            schema: "plutus",
            table: "backtest_position",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,6)",
            oldPrecision: 18,
            oldScale: 6
        );
    }
}

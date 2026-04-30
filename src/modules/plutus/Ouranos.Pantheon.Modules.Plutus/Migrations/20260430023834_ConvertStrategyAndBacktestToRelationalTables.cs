using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Migrations;

/// <inheritdoc />
public partial class ConvertStrategyAndBacktestToRelationalTables : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "strategies",
            schema: "plutus",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                market_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                description = table.Column<string>(type: "text", nullable: true),
                type = table.Column<int>(type: "integer", nullable: false),
                configuration_buy_threshold = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                configuration_sell_threshold = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                configuration_forecast_movement_threshold = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                configuration_forecast_horizon_days = table.Column<int>(type: "integer", nullable: true),
                configuration_deviation_multiplier = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                configuration_mean_time_frame_value = table.Column<int>(type: "integer", nullable: true),
                configuration_min_margin_percent = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                configuration_max_positions = table.Column<int>(type: "integer", nullable: true),
                configuration_max_position_percent = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                configuration_hold_period_days = table.Column<int>(type: "integer", nullable: true),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_strategies", x => x.id);
                table.ForeignKey(
                    name: "fk_strategies_markets_market_id",
                    column: x => x.market_id,
                    principalSchema: "plutus",
                    principalTable: "markets",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "backtests",
            schema: "plutus",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                strategy_id = table.Column<Guid>(type: "uuid", nullable: false),
                market_id = table.Column<Guid>(type: "uuid", nullable: false),
                start_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                end_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                budget = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                status = table.Column<int>(type: "integer", nullable: false),
                results_total_return = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                results_total_return_percent = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                results_max_drawdown = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                results_max_drawdown_percent = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                results_win_rate = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                results_total_trades = table.Column<int>(type: "integer", nullable: true),
                results_winning_trades = table.Column<int>(type: "integer", nullable: true),
                results_losing_trades = table.Column<int>(type: "integer", nullable: true),
                results_sharpe_ratio = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                results_average_trade_return = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                results_best_trade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                results_worst_trade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                results_final_balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                error_message = table.Column<string>(type: "text", nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_backtests", x => x.id);
                table.ForeignKey(
                    name: "fk_backtests_strategies_strategy_id",
                    column: x => x.strategy_id,
                    principalSchema: "plutus",
                    principalTable: "strategies",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "composite_component",
            schema: "plutus",
            columns: table => new
            {
                strategy_configuration_strategy_id = table.Column<Guid>(type: "uuid", nullable: false),
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                strategy_id = table.Column<Guid>(type: "uuid", nullable: false),
                type = table.Column<int>(type: "integer", nullable: false),
                weight = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_composite_component", x => new { x.strategy_configuration_strategy_id, x.id });
                table.ForeignKey(
                    name: "fk_composite_component_strategies_strategy_configuration_strat",
                    column: x => x.strategy_configuration_strategy_id,
                    principalSchema: "plutus",
                    principalTable: "strategies",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "signal_weight",
            schema: "plutus",
            columns: table => new
            {
                strategy_configuration_strategy_id = table.Column<Guid>(type: "uuid", nullable: false),
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "backtest_position",
            schema: "plutus",
            columns: table => new
            {
                backtest_results_backtest_id = table.Column<Guid>(type: "uuid", nullable: false),
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                symbol_id = table.Column<string>(type: "text", nullable: false),
                symbol_name = table.Column<string>(type: "text", nullable: false),
                entry_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                exit_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                volume = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                profit_loss = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                return_percent = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                entry_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                exit_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_backtest_position", x => new { x.backtest_results_backtest_id, x.id });
                table.ForeignKey(
                    name: "fk_backtest_position_backtests_backtest_results_backtest_id",
                    column: x => x.backtest_results_backtest_id,
                    principalSchema: "plutus",
                    principalTable: "backtests",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_backtests_status",
            schema: "plutus",
            table: "backtests",
            column: "status");

        migrationBuilder.CreateIndex(
            name: "ix_backtests_strategy_id",
            schema: "plutus",
            table: "backtests",
            column: "strategy_id");

        migrationBuilder.CreateIndex(
            name: "ix_strategies_market_id",
            schema: "plutus",
            table: "strategies",
            column: "market_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "backtest_position",
            schema: "plutus");

        migrationBuilder.DropTable(
            name: "composite_component",
            schema: "plutus");

        migrationBuilder.DropTable(
            name: "signal_weight",
            schema: "plutus");

        migrationBuilder.DropTable(
            name: "backtests",
            schema: "plutus");

        migrationBuilder.DropTable(
            name: "strategies",
            schema: "plutus");
    }
}

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Migrations;

/// <inheritdoc />
public partial class ConsolidateStrategyConfiguration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "configuration",
            schema: "plutus",
            table: "strategies",
            type: "jsonb",
            nullable: true
        );

        migrationBuilder.AddColumn<string>(
            name: "results_optimized_configuration",
            schema: "plutus",
            table: "backtests",
            type: "jsonb",
            nullable: true
        );

        migrationBuilder.AddColumn<string>(
            name: "run_settings",
            schema: "plutus",
            table: "backtests",
            type: "jsonb",
            nullable: true
        );

        migrationBuilder.Sql(
            """
            UPDATE plutus.strategies s
            SET configuration = jsonb_strip_nulls(jsonb_build_object(
                'mode', 'Swing',
                'inputWeights', COALESCE(
                    (SELECT jsonb_agg(jsonb_build_object('kind', CASE w.kind WHEN 1 THEN 'SignalTaxAdjustedRoi' WHEN 2 THEN 'SignalVolumeAnomaly' WHEN 3 THEN 'SignalTrendMomentum' WHEN 4 THEN 'SignalBollingerBands' WHEN 5 THEN 'SignalRsi' WHEN 6 THEN 'SignalMovingAverageCrossover' WHEN 7 THEN 'SignalPriceVelocity' END, 'weight', w.weight) ORDER BY w.kind)
                     FROM plutus.strategies_input_weights w
                     WHERE w.strategy_id = s.id),
                    '[]'::jsonb),
                'buyThreshold', s.thresholds_buy_threshold,
                'sellThreshold', s.thresholds_sell_threshold,
                'maxPositions', s.trading_configuration_max_positions,
                'maxPositionPercent', s.trading_configuration_max_position_percent,
                'holdPeriodDays', s.trading_configuration_hold_period_days));

            UPDATE plutus.backtests b
            SET results_optimized_configuration = jsonb_strip_nulls(jsonb_build_object(
                'mode', 'Swing',
                'inputWeights', COALESCE(
                    (SELECT jsonb_agg(jsonb_build_object('kind', CASE w.kind WHEN 1 THEN 'SignalTaxAdjustedRoi' WHEN 2 THEN 'SignalVolumeAnomaly' WHEN 3 THEN 'SignalTrendMomentum' WHEN 4 THEN 'SignalBollingerBands' WHEN 5 THEN 'SignalRsi' WHEN 6 THEN 'SignalMovingAverageCrossover' WHEN 7 THEN 'SignalPriceVelocity' END, 'weight', w.weight) ORDER BY w.kind)
                     FROM plutus.backtests_optimized_input_weights w
                     WHERE w.backtest_results_backtest_id = b.id),
                    '[]'::jsonb),
                'buyThreshold', b.results_optimized_thresholds_buy_threshold,
                'sellThreshold', b.results_optimized_thresholds_sell_threshold,
                'maxPositions', b.results_optimized_configuration_max_positions,
                'maxPositionPercent', b.results_optimized_configuration_max_position_percent,
                'holdPeriodDays', b.results_optimized_configuration_hold_period_days))
            WHERE b.kind = 'Optimization' AND b.status = 3;

            UPDATE plutus.backtests b
            SET run_settings = CASE WHEN b.kind = 'Optimization'
                THEN '{"volumeParticipationRate":0.1,"slippageMultiplier":0.05,"optimization":{"generations":30,"populationSize":50,"drawdownWeight":0.5,"outSampleRatio":0.2,"minTradesPerMonth":10}}'::jsonb
                ELSE '{"volumeParticipationRate":0.1,"slippageMultiplier":0.05}'::jsonb
            END;
            """
        );

        migrationBuilder.AlterColumn<string>(
            name: "configuration",
            schema: "plutus",
            table: "strategies",
            type: "jsonb",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "jsonb",
            oldNullable: true
        );

        migrationBuilder.AlterColumn<string>(
            name: "run_settings",
            schema: "plutus",
            table: "backtests",
            type: "jsonb",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "jsonb",
            oldNullable: true
        );

        migrationBuilder.DropTable(name: "backtests_optimized_input_weights", schema: "plutus");

        migrationBuilder.DropTable(name: "strategies_input_weights", schema: "plutus");

        migrationBuilder.DropColumn(
            name: "thresholds_buy_threshold",
            schema: "plutus",
            table: "strategies"
        );

        migrationBuilder.DropColumn(
            name: "thresholds_sell_threshold",
            schema: "plutus",
            table: "strategies"
        );

        migrationBuilder.DropColumn(
            name: "trading_configuration_hold_period_days",
            schema: "plutus",
            table: "strategies"
        );

        migrationBuilder.DropColumn(
            name: "trading_configuration_max_position_percent",
            schema: "plutus",
            table: "strategies"
        );

        migrationBuilder.DropColumn(
            name: "trading_configuration_max_positions",
            schema: "plutus",
            table: "strategies"
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
            name: "results_optimized_thresholds_buy_threshold",
            schema: "plutus",
            table: "backtests"
        );

        migrationBuilder.DropColumn(
            name: "results_optimized_thresholds_sell_threshold",
            schema: "plutus",
            table: "backtests"
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "configuration", schema: "plutus", table: "strategies");

        migrationBuilder.DropColumn(
            name: "results_optimized_configuration",
            schema: "plutus",
            table: "backtests"
        );

        migrationBuilder.DropColumn(name: "run_settings", schema: "plutus", table: "backtests");

        migrationBuilder.AddColumn<decimal>(
            name: "thresholds_buy_threshold",
            schema: "plutus",
            table: "strategies",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: true
        );

        migrationBuilder.AddColumn<decimal>(
            name: "thresholds_sell_threshold",
            schema: "plutus",
            table: "strategies",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: true
        );

        migrationBuilder.AddColumn<int>(
            name: "trading_configuration_hold_period_days",
            schema: "plutus",
            table: "strategies",
            type: "integer",
            nullable: true
        );

        migrationBuilder.AddColumn<decimal>(
            name: "trading_configuration_max_position_percent",
            schema: "plutus",
            table: "strategies",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: true
        );

        migrationBuilder.AddColumn<int>(
            name: "trading_configuration_max_positions",
            schema: "plutus",
            table: "strategies",
            type: "integer",
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
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: true
        );

        migrationBuilder.AddColumn<int>(
            name: "results_optimized_configuration_max_positions",
            schema: "plutus",
            table: "backtests",
            type: "integer",
            nullable: true
        );

        migrationBuilder.AddColumn<decimal>(
            name: "results_optimized_thresholds_buy_threshold",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: true
        );

        migrationBuilder.AddColumn<decimal>(
            name: "results_optimized_thresholds_sell_threshold",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: true
        );

        migrationBuilder.CreateTable(
            name: "backtests_optimized_input_weights",
            schema: "plutus",
            columns: table => new
            {
                backtest_results_backtest_id = table.Column<Guid>(type: "uuid", nullable: false),
                id = table
                    .Column<int>(type: "integer", nullable: false)
                    .Annotation(
                        "Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                    ),
                kind = table.Column<int>(type: "integer", nullable: false),
                weight = table.Column<decimal>(
                    type: "numeric(18,6)",
                    precision: 18,
                    scale: 6,
                    nullable: false
                ),
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "pk_backtests_optimized_input_weights",
                    x => new { x.backtest_results_backtest_id, x.id }
                );
                table.ForeignKey(
                    name: "fk_backtests_optimized_input_weights_backtests_backtest_result",
                    column: x => x.backtest_results_backtest_id,
                    principalSchema: "plutus",
                    principalTable: "backtests",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateTable(
            name: "strategies_input_weights",
            schema: "plutus",
            columns: table => new
            {
                strategy_id = table.Column<Guid>(type: "uuid", nullable: false),
                id = table
                    .Column<int>(type: "integer", nullable: false)
                    .Annotation(
                        "Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                    ),
                kind = table.Column<int>(type: "integer", nullable: false),
                weight = table.Column<decimal>(
                    type: "numeric(18,6)",
                    precision: 18,
                    scale: 6,
                    nullable: false
                ),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_strategies_input_weights", x => new { x.strategy_id, x.id });
                table.ForeignKey(
                    name: "fk_strategies_input_weights_strategies_strategy_id",
                    column: x => x.strategy_id,
                    principalSchema: "plutus",
                    principalTable: "strategies",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateIndex(
            name: "ix_backtests_optimized_input_weights_backtest_results_backtest",
            schema: "plutus",
            table: "backtests_optimized_input_weights",
            columns: new[] { "backtest_results_backtest_id", "kind" },
            unique: true
        );

        migrationBuilder.CreateIndex(
            name: "ix_strategies_input_weights_strategy_id_kind",
            schema: "plutus",
            table: "strategies_input_weights",
            columns: new[] { "strategy_id", "kind" },
            unique: true
        );
    }
}

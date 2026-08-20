using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Migrations;

/// <summary>
///     Converts the forecasts table to a TimescaleDB hypertable (collapsing the
///     forecasts_predictions child table into a predictions JSON column) and
///     consolidates the five strategy types into a single unified model with a
///     weighted vector of signal inputs and shared buy/sell thresholds. Also adds
///     walk-forward out-of-sample result columns, a turnover-rate column, and an
///     is-validated flag on backtests, and removes the three non-signal strategy
///     inputs (ForecastMomentum, MeanReversion, RecipeArbitrage) from the unified
///     model.
///     <para>Down is lossy: the strategy type discriminator and the non-signal input weights are unrecoverable, and the forecasts hypertable conversion is not reversible.</para>
/// </summary>
public partial class ConsolidateStrategyAndForecastModels : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ConvertForecastsToHypertable(migrationBuilder);

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
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
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
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
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

        AddStrategyThresholdColumns(migrationBuilder);
        AddBacktestResultColumns(migrationBuilder);

        MigrateStrategies(migrationBuilder);
        MigrateBacktests(migrationBuilder);

        DropOldStrategyColumns(migrationBuilder);
        DropOldBacktestColumns(migrationBuilder);
        migrationBuilder.DropTable(name: "composite_component", schema: "plutus");
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Lossy with respect to the strategy type discriminator and the non-signal
    ///     input weights: the unified model stores only signal input weights, so the
    ///     original type=2/3/4/5 assignments and their family-specific thresholds
    ///     cannot be reconstructed. Optimized backtest configurations are also
    ///     unrecoverable (the backtests_optimized_input_weights table is dropped
    ///     without re-projecting onto the legacy results_optimized_*_weight columns).
    ///     The forecasts hypertable conversion is not reversible at all. This is
    ///     acceptable for the deployment profile (rollbacks are rare and operators can
    ///     re-run an optimization to regenerate optimized weights).
    /// </remarks>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "composite_component",
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
                type = table.Column<int>(type: "integer", nullable: false),
                weight = table.Column<decimal>(
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: false
                ),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_composite_component", x => new { x.strategy_id, x.id });
                table.ForeignKey(
                    name: "fk_composite_component_strategies_strategy_id",
                    column: x => x.strategy_id,
                    principalSchema: "plutus",
                    principalTable: "strategies",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        RestoreOldStrategyColumns(migrationBuilder);
        RestoreOldBacktestColumns(migrationBuilder);

        // Lossy: the unified model has no type discriminator, so every strategy is
        // restored as type=1 (SignalWeighted). The original type=2/3/4/5 assignments
        // are unrecoverable after consolidation.
        migrationBuilder.Sql(
            """
            UPDATE plutus.strategies SET type = 1;
            """
        );

        migrationBuilder.Sql(
            """
            UPDATE plutus.strategies
            SET signal_weighted_config_buy_threshold = thresholds_buy_threshold,
                signal_weighted_config_sell_threshold = thresholds_sell_threshold
            """
        );

        migrationBuilder.Sql(
            """
            UPDATE plutus.strategies s
            SET signal_weighted_config_tax_adjusted_roi_weight = COALESCE(sw.weight, 0)
            FROM plutus.strategies_input_weights sw
            WHERE sw.strategy_id = s.id AND sw.kind = 1
            """
        );
        migrationBuilder.Sql(
            """
            UPDATE plutus.strategies s
            SET signal_weighted_config_volume_anomaly_weight = COALESCE(sw.weight, 0)
            FROM plutus.strategies_input_weights sw
            WHERE sw.strategy_id = s.id AND sw.kind = 2
            """
        );
        migrationBuilder.Sql(
            """
            UPDATE plutus.strategies s
            SET signal_weighted_config_trend_momentum_weight = COALESCE(sw.weight, 0)
            FROM plutus.strategies_input_weights sw
            WHERE sw.strategy_id = s.id AND sw.kind = 3
            """
        );
        migrationBuilder.Sql(
            """
            UPDATE plutus.strategies s
            SET signal_weighted_config_bollinger_bands_weight = COALESCE(sw.weight, 0)
            FROM plutus.strategies_input_weights sw
            WHERE sw.strategy_id = s.id AND sw.kind = 4
            """
        );
        migrationBuilder.Sql(
            """
            UPDATE plutus.strategies s
            SET signal_weighted_config_rsi_weight = COALESCE(sw.weight, 0)
            FROM plutus.strategies_input_weights sw
            WHERE sw.strategy_id = s.id AND sw.kind = 5
            """
        );
        migrationBuilder.Sql(
            """
            UPDATE plutus.strategies s
            SET signal_weighted_config_moving_average_crossover_weight = COALESCE(sw.weight, 0)
            FROM plutus.strategies_input_weights sw
            WHERE sw.strategy_id = s.id AND sw.kind = 6
            """
        );
        migrationBuilder.Sql(
            """
            UPDATE plutus.strategies s
            SET signal_weighted_config_price_velocity_weight = COALESCE(sw.weight, 0)
            FROM plutus.strategies_input_weights sw
            WHERE sw.strategy_id = s.id AND sw.kind = 7
            """
        );

        DropNewStrategyColumns(migrationBuilder);
        DropNewBacktestColumns(migrationBuilder);
        migrationBuilder.DropTable(name: "backtests_optimized_input_weights", schema: "plutus");
        migrationBuilder.DropTable(name: "strategies_input_weights", schema: "plutus");
    }

    private static void ConvertForecastsToHypertable(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE plutus.forecasts
                ADD COLUMN predictions jsonb NOT NULL DEFAULT '[]'::jsonb;
            """
        );

        // Backfill before dropping the child table. JSON keys are PascalCase to
        // match EF Core's default JSON property naming (EFCore.NamingConventions
        // does not transform JSON property names).
        migrationBuilder.Sql(
            """
            UPDATE plutus.forecasts f
            SET predictions = COALESCE(
                (
                    SELECT jsonb_agg(jsonb_build_object(
                        'AveragePrice', p.average_price,
                        'MinPrice',       p.min_price,
                        'MaxPrice',       p.max_price,
                        'Volume',          p.volume
                    ) ORDER BY p.id)
                    FROM plutus.forecasts_predictions p
                    WHERE p.forecast_id = f.id
                ),
                '[]'::jsonb
            );
            """
        );

        migrationBuilder.DropTable(name: "forecasts_predictions", schema: "plutus");

        // The original FK-convention single-column index is superseded by the
        // composite (symbol_id, created_at) index below; the new index's leading
        // column already serves FK lookups, so drop the redundant one to match the
        // updated EF Core model snapshot.
        migrationBuilder.DropIndex(
            name: "ix_forecasts_symbol_id",
            schema: "plutus",
            table: "forecasts"
        );

        migrationBuilder.CreateIndex(
            name: "ix_forecasts_symbol_id_created_at",
            schema: "plutus",
            table: "forecasts",
            columns: new[] { "symbol_id", "created_at" }
        );

        // TimescaleDB requires the partition column in the PK.
        migrationBuilder.Sql(
            """
            ALTER TABLE plutus.forecasts DROP CONSTRAINT pk_forecasts;
            ALTER TABLE plutus.forecasts
                ADD CONSTRAINT pk_forecasts PRIMARY KEY (id, created_at);
            """
        );

        migrationBuilder.Sql(
            """
            SELECT create_hypertable('plutus.forecasts', 'created_at',
                chunk_time_interval => INTERVAL '7 days',
                migrate_data => TRUE,
                if_not_exists => TRUE);

            SELECT add_retention_policy('plutus.forecasts',
                drop_after => INTERVAL '180 days',
                schedule_interval => INTERVAL '1 day',
                if_not_exists => TRUE);

            ALTER TABLE plutus.forecasts SET (
                timescaledb.compress,
                timescaledb.compress_segmentby = 'symbol_id'
            );
            """
        );

        migrationBuilder.Sql(
            """
            SELECT add_compression_policy('plutus.forecasts',
                INTERVAL '7 days',
                if_not_exists => TRUE);
            """,
            suppressTransaction: true
        );
    }

    private static void AddStrategyThresholdColumns(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "thresholds_buy_threshold",
            schema: "plutus",
            table: "strategies",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );

        migrationBuilder.AddColumn<decimal>(
            name: "thresholds_sell_threshold",
            schema: "plutus",
            table: "strategies",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );
    }

    private static void AddBacktestResultColumns(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "results_optimized_thresholds_buy_threshold",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );

        migrationBuilder.AddColumn<decimal>(
            name: "results_optimized_thresholds_sell_threshold",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );

        migrationBuilder.AddColumn<decimal>(
            name: "results_turnover_rate",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );

        migrationBuilder.AddColumn<bool>(
            name: "results_is_validated",
            schema: "plutus",
            table: "backtests",
            type: "boolean",
            nullable: true
        );

        var oosCols = new (string Name, string Type, int? Precision, int? Scale)[]
        {
            ("results_out_sample_results_total_return", "numeric(18,2)", 18, 2),
            ("results_out_sample_results_total_return_percent", "numeric(18,2)", 18, 2),
            ("results_out_sample_results_max_drawdown", "numeric(18,2)", 18, 2),
            ("results_out_sample_results_max_drawdown_percent", "numeric(18,2)", 18, 2),
            ("results_out_sample_results_win_rate", "numeric(18,2)", 18, 2),
            ("results_out_sample_results_total_trades", "integer", null, null),
            ("results_out_sample_results_winning_trades", "integer", null, null),
            ("results_out_sample_results_losing_trades", "integer", null, null),
            ("results_out_sample_results_sharpe_ratio", "numeric(18,2)", 18, 2),
            ("results_out_sample_results_sortino_ratio", "numeric(18,2)", 18, 2),
            ("results_out_sample_results_calmar_ratio", "numeric(18,2)", 18, 2),
            ("results_out_sample_results_cagr", "numeric(18,2)", 18, 2),
            ("results_out_sample_results_profit_factor", "numeric(18,2)", 18, 2),
            ("results_out_sample_results_expectancy", "numeric(18,2)", 18, 2),
            ("results_out_sample_results_average_trade_return", "numeric(18,2)", 18, 2),
            ("results_out_sample_results_best_trade", "numeric(18,2)", 18, 2),
            ("results_out_sample_results_worst_trade", "numeric(18,2)", 18, 2),
            ("results_out_sample_results_final_balance", "numeric(18,2)", 18, 2),
        };

        foreach (var (name, type, precision, scale) in oosCols)
        {
            if (precision.HasValue)
            {
                migrationBuilder.AddColumn<decimal>(
                    name: name,
                    schema: "plutus",
                    table: "backtests",
                    type: type,
                    precision: precision,
                    scale: scale,
                    nullable: true
                );
            }
            else
            {
                migrationBuilder.AddColumn<int>(
                    name: name,
                    schema: "plutus",
                    table: "backtests",
                    type: type,
                    nullable: true
                );
            }
        }
    }

    private static void MigrateStrategies(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            INSERT INTO plutus.strategies_input_weights (strategy_id, id, kind, weight)
            SELECT s.id, ROW_NUMBER() OVER (PARTITION BY s.id ORDER BY kind), kind, weight
            FROM (
                SELECT id, 1 AS kind, COALESCE(signal_weighted_config_tax_adjusted_roi_weight, 0) AS weight FROM plutus.strategies WHERE type = 1
                UNION ALL
                SELECT id, 2 AS kind, COALESCE(signal_weighted_config_volume_anomaly_weight, 0) AS weight FROM plutus.strategies WHERE type = 1
                UNION ALL
                SELECT id, 3 AS kind, COALESCE(signal_weighted_config_trend_momentum_weight, 0) AS weight FROM plutus.strategies WHERE type = 1
                UNION ALL
                SELECT id, 4 AS kind, COALESCE(signal_weighted_config_bollinger_bands_weight, 0) AS weight FROM plutus.strategies WHERE type = 1
                UNION ALL
                SELECT id, 5 AS kind, COALESCE(signal_weighted_config_rsi_weight, 0) AS weight FROM plutus.strategies WHERE type = 1
                UNION ALL
                SELECT id, 6 AS kind, COALESCE(signal_weighted_config_moving_average_crossover_weight, 0) AS weight FROM plutus.strategies WHERE type = 1
                UNION ALL
                SELECT id, 7 AS kind, COALESCE(signal_weighted_config_price_velocity_weight, 0) AS weight FROM plutus.strategies WHERE type = 1
            ) s
            WHERE s.weight <> 0
            """
        );

        migrationBuilder.Sql(
            """
            UPDATE plutus.strategies
            SET thresholds_buy_threshold = signal_weighted_config_buy_threshold,
                thresholds_sell_threshold = signal_weighted_config_sell_threshold
            WHERE type IN (1, 5)
            """
        );

        // Composite (type=5): the Composite backtest path was non-functional prior to
        // this migration (ScoreSymbolsStep never populated StrategyScoreContext.Components),
        // so production Composite data is unlikely. This handles it defensively. Only
        // signal-typed components (type=1) are projected; the non-signal component
        // types are being removed from the model.
        migrationBuilder.Sql(
            """
            INSERT INTO plutus.strategies_input_weights (strategy_id, id, kind, weight)
            SELECT parent.id, ROW_NUMBER() OVER (PARTITION BY parent.id ORDER BY kinds.kind), kinds.kind, kinds.weight
            FROM plutus.composite_component cc
            JOIN plutus.strategies parent ON parent.id = cc.strategy_id AND parent.type = 5
            JOIN LATERAL (
                SELECT 1 AS kind, cc.weight * COALESCE(parent.signal_weighted_config_tax_adjusted_roi_weight, 1.0) AS weight WHERE cc.type = 1
                UNION ALL
                SELECT 2, cc.weight * COALESCE(parent.signal_weighted_config_volume_anomaly_weight, 1.0) WHERE cc.type = 1
                UNION ALL
                SELECT 3, cc.weight * COALESCE(parent.signal_weighted_config_trend_momentum_weight, 1.0) WHERE cc.type = 1
                UNION ALL
                SELECT 4, cc.weight * COALESCE(parent.signal_weighted_config_bollinger_bands_weight, 1.0) WHERE cc.type = 1
                UNION ALL
                SELECT 5, cc.weight * COALESCE(parent.signal_weighted_config_rsi_weight, 1.0) WHERE cc.type = 1
                UNION ALL
                SELECT 6, cc.weight * COALESCE(parent.signal_weighted_config_moving_average_crossover_weight, 1.0) WHERE cc.type = 1
                UNION ALL
                SELECT 7, cc.weight * COALESCE(parent.signal_weighted_config_price_velocity_weight, 1.0) WHERE cc.type = 1
            ) kinds ON kinds.weight <> 0
            """
        );

        // Strategies of the removed non-signal types (2, 3, 4) and any strategy left
        // without weights get a default equal-weight signal vector so no strategy is
        // left in an unusable state.
        migrationBuilder.Sql(
            """
            INSERT INTO plutus.strategies_input_weights (strategy_id, id, kind, weight)
            SELECT s.id, k.kind, k.kind, 1.0
            FROM plutus.strategies s
            CROSS JOIN (VALUES (1),(2),(3),(4),(5),(6),(7)) AS k(kind)
            WHERE NOT EXISTS (
                SELECT 1 FROM plutus.strategies_input_weights siw
                WHERE siw.strategy_id = s.id
            )
            """
        );
    }

    private static void MigrateBacktests(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            INSERT INTO plutus.backtests_optimized_input_weights (backtest_results_backtest_id, id, kind, weight)
            SELECT b.id, ROW_NUMBER() OVER (PARTITION BY b.id ORDER BY kind), kind, weight
            FROM (
                SELECT b.id, 1 AS kind, COALESCE(b.results_optimized_signal_weighted_config_tax_adjusted_roi_weight, 0) AS weight
                FROM plutus.backtests b JOIN plutus.strategies s ON s.id = b.strategy_id WHERE s.type = 1
                UNION ALL
                SELECT b.id, 2, COALESCE(b.results_optimized_signal_weighted_config_volume_anomaly_weight, 0) FROM plutus.backtests b JOIN plutus.strategies s ON s.id = b.strategy_id WHERE s.type = 1
                UNION ALL
                SELECT b.id, 3, COALESCE(b.results_optimized_signal_weighted_config_trend_momentum_weight, 0) FROM plutus.backtests b JOIN plutus.strategies s ON s.id = b.strategy_id WHERE s.type = 1
                UNION ALL
                SELECT b.id, 4, COALESCE(b.results_optimized_signal_weighted_config_bollinger_bands_weight, 0) FROM plutus.backtests b JOIN plutus.strategies s ON s.id = b.strategy_id WHERE s.type = 1
                UNION ALL
                SELECT b.id, 5, COALESCE(b.results_optimized_signal_weighted_config_rsi_weight, 0) FROM plutus.backtests b JOIN plutus.strategies s ON s.id = b.strategy_id WHERE s.type = 1
                UNION ALL
                SELECT b.id, 6, COALESCE(b.results_optimized_signal_weighted_config_moving_average_crossover_we, 0) FROM plutus.backtests b JOIN plutus.strategies s ON s.id = b.strategy_id WHERE s.type = 1
                UNION ALL
                SELECT b.id, 7, COALESCE(b.results_optimized_signal_weighted_config_price_velocity_weight, 0) FROM plutus.backtests b JOIN plutus.strategies s ON s.id = b.strategy_id WHERE s.type = 1
            ) b
            WHERE b.weight <> 0
            """
        );
        migrationBuilder.Sql(
            """
            UPDATE plutus.backtests b
            SET results_optimized_thresholds_buy_threshold = b.results_optimized_signal_weighted_config_buy_threshold,
                results_optimized_thresholds_sell_threshold = b.results_optimized_signal_weighted_config_sell_threshold
            FROM plutus.strategies s
            WHERE s.id = b.strategy_id AND s.type = 1
            """
        );

        // Scope to completed backtests via the non-null results_total_return
        // sentinel: TotalReturn is a non-nullable decimal set by Complete(), so its
        // column is non-null only when the Results owned entity exists. Backfilling
        // two columns on a Pending/Running/Failed row (where every results_* is NULL)
        // would break EF Core's all-columns-NULL => navigation-null detection and throw
        // when materializing the still-NULL results_total_return.
        migrationBuilder.Sql(
            """
            UPDATE plutus.backtests
            SET results_turnover_rate = 0,
                results_is_validated = false
            WHERE results_total_return IS NOT NULL
              AND results_turnover_rate IS NULL
            """
        );
    }

    private static void DropOldStrategyColumns(MigrationBuilder migrationBuilder)
    {
        var columns = new[]
        {
            "signal_weighted_config_buy_threshold",
            "signal_weighted_config_sell_threshold",
            "signal_weighted_config_tax_adjusted_roi_weight",
            "signal_weighted_config_volume_anomaly_weight",
            "signal_weighted_config_trend_momentum_weight",
            "signal_weighted_config_bollinger_bands_weight",
            "signal_weighted_config_rsi_weight",
            "signal_weighted_config_moving_average_crossover_weight",
            "signal_weighted_config_price_velocity_weight",
            "forecast_momentum_config_forecast_movement_threshold",
            "forecast_momentum_config_forecast_horizon_days",
            "mean_reversion_config_deviation_multiplier",
            "mean_reversion_config_mean_time_frame_value",
            "recipe_arbitrage_config_min_margin_percent",
            "type",
        };

        foreach (var column in columns)
        {
            migrationBuilder.DropColumn(name: column, schema: "plutus", table: "strategies");
        }
    }

    private static void DropOldBacktestColumns(MigrationBuilder migrationBuilder)
    {
        var columns = new[]
        {
            "results_optimized_signal_weighted_config_buy_threshold",
            "results_optimized_signal_weighted_config_sell_threshold",
            "results_optimized_signal_weighted_config_tax_adjusted_roi_weight",
            "results_optimized_signal_weighted_config_volume_anomaly_weight",
            "results_optimized_signal_weighted_config_trend_momentum_weight",
            "results_optimized_signal_weighted_config_bollinger_bands_weight",
            "results_optimized_signal_weighted_config_rsi_weight",
            "results_optimized_signal_weighted_config_moving_average_crossover_we",
            "results_optimized_signal_weighted_config_price_velocity_weight",
            "results_optimized_forecast_momentum_config_forecast_movement_thresh",
            "results_optimized_forecast_momentum_config_forecast_horizon_days",
            "results_optimized_mean_reversion_config_deviation_multiplier",
            "results_optimized_mean_reversion_config_mean_time_frame_value",
            "results_optimized_recipe_arbitrage_config_min_margin_percent",
        };

        foreach (var column in columns)
        {
            migrationBuilder.DropColumn(name: column, schema: "plutus", table: "backtests");
        }
    }

    private static void RestoreOldStrategyColumns(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "recipe_arbitrage_config_min_margin_percent",
            schema: "plutus",
            table: "strategies",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );
        var signalWeightCols = new[]
        {
            "signal_weighted_config_bollinger_bands_weight",
            "signal_weighted_config_moving_average_crossover_weight",
            "signal_weighted_config_price_velocity_weight",
            "signal_weighted_config_rsi_weight",
            "signal_weighted_config_tax_adjusted_roi_weight",
            "signal_weighted_config_trend_momentum_weight",
            "signal_weighted_config_volume_anomaly_weight",
            "signal_weighted_config_buy_threshold",
            "signal_weighted_config_sell_threshold",
        };
        foreach (var col in signalWeightCols)
        {
            migrationBuilder.AddColumn<decimal>(
                name: col,
                schema: "plutus",
                table: "strategies",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true
            );
        }
        migrationBuilder.AddColumn<decimal>(
            name: "forecast_momentum_config_forecast_movement_threshold",
            schema: "plutus",
            table: "strategies",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );
        migrationBuilder.AddColumn<int>(
            name: "forecast_momentum_config_forecast_horizon_days",
            schema: "plutus",
            table: "strategies",
            type: "integer",
            nullable: true
        );
        migrationBuilder.AddColumn<decimal>(
            name: "mean_reversion_config_deviation_multiplier",
            schema: "plutus",
            table: "strategies",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );
        migrationBuilder.AddColumn<int>(
            name: "mean_reversion_config_mean_time_frame_value",
            schema: "plutus",
            table: "strategies",
            type: "integer",
            nullable: true
        );
        migrationBuilder.AddColumn<int>(
            name: "type",
            schema: "plutus",
            table: "strategies",
            type: "integer",
            nullable: false,
            defaultValue: 1
        );
    }

    private static void RestoreOldBacktestColumns(MigrationBuilder migrationBuilder)
    {
        var columns = new (string Name, string Type, int? Precision, int? Scale)[]
        {
            ("results_optimized_signal_weighted_config_buy_threshold", "numeric(18,2)", 18, 2),
            ("results_optimized_signal_weighted_config_sell_threshold", "numeric(18,2)", 18, 2),
            (
                "results_optimized_signal_weighted_config_tax_adjusted_roi_weight",
                "numeric(18,2)",
                18,
                2
            ),
            (
                "results_optimized_signal_weighted_config_volume_anomaly_weight",
                "numeric(18,2)",
                18,
                2
            ),
            (
                "results_optimized_signal_weighted_config_trend_momentum_weight",
                "numeric(18,2)",
                18,
                2
            ),
            (
                "results_optimized_signal_weighted_config_bollinger_bands_weight",
                "numeric(18,2)",
                18,
                2
            ),
            ("results_optimized_signal_weighted_config_rsi_weight", "numeric(18,2)", 18, 2),
            (
                "results_optimized_signal_weighted_config_moving_average_crossover_we",
                "numeric(18,2)",
                18,
                2
            ),
            (
                "results_optimized_signal_weighted_config_price_velocity_weight",
                "numeric(18,2)",
                18,
                2
            ),
            (
                "results_optimized_forecast_momentum_config_forecast_movement_thresh",
                "numeric(18,2)",
                18,
                2
            ),
            (
                "results_optimized_forecast_momentum_config_forecast_horizon_days",
                "integer",
                null,
                null
            ),
            (
                "results_optimized_mean_reversion_config_deviation_multiplier",
                "numeric(18,2)",
                18,
                2
            ),
            (
                "results_optimized_mean_reversion_config_mean_time_frame_value",
                "integer",
                null,
                null
            ),
            (
                "results_optimized_recipe_arbitrage_config_min_margin_percent",
                "numeric(18,2)",
                18,
                2
            ),
        };

        foreach (var (name, type, precision, scale) in columns)
        {
            if (precision.HasValue)
            {
                migrationBuilder.AddColumn<decimal>(
                    name: name,
                    schema: "plutus",
                    table: "backtests",
                    type: type,
                    precision: precision,
                    scale: scale,
                    nullable: true
                );
            }
            else
            {
                migrationBuilder.AddColumn<int>(
                    name: name,
                    schema: "plutus",
                    table: "backtests",
                    type: type,
                    nullable: true
                );
            }
        }
    }

    private static void DropNewStrategyColumns(MigrationBuilder migrationBuilder)
    {
        var columns = new[] { "thresholds_buy_threshold", "thresholds_sell_threshold" };
        foreach (var column in columns)
        {
            migrationBuilder.DropColumn(name: column, schema: "plutus", table: "strategies");
        }
    }

    private static void DropNewBacktestColumns(MigrationBuilder migrationBuilder)
    {
        var columns = new[]
        {
            "results_optimized_thresholds_buy_threshold",
            "results_optimized_thresholds_sell_threshold",
            "results_turnover_rate",
            "results_is_validated",
            "results_out_sample_results_total_return",
            "results_out_sample_results_total_return_percent",
            "results_out_sample_results_max_drawdown",
            "results_out_sample_results_max_drawdown_percent",
            "results_out_sample_results_win_rate",
            "results_out_sample_results_total_trades",
            "results_out_sample_results_winning_trades",
            "results_out_sample_results_losing_trades",
            "results_out_sample_results_sharpe_ratio",
            "results_out_sample_results_sortino_ratio",
            "results_out_sample_results_calmar_ratio",
            "results_out_sample_results_cagr",
            "results_out_sample_results_profit_factor",
            "results_out_sample_results_expectancy",
            "results_out_sample_results_average_trade_return",
            "results_out_sample_results_best_trade",
            "results_out_sample_results_worst_trade",
            "results_out_sample_results_final_balance",
        };
        foreach (var column in columns)
        {
            migrationBuilder.DropColumn(name: column, schema: "plutus", table: "backtests");
        }
    }
}

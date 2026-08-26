using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Migrations;

/// <inheritdoc />
public partial class ConvertForecastEvaluatorToView : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_forecast_records_target_at_evaluated_at",
            schema: "plutus",
            table: "forecast_records"
        );

        migrationBuilder.DropColumn(
            name: "actual_average_price",
            schema: "plutus",
            table: "forecast_records"
        );

        migrationBuilder.DropColumn(
            name: "actual_max_price",
            schema: "plutus",
            table: "forecast_records"
        );

        migrationBuilder.DropColumn(
            name: "actual_min_price",
            schema: "plutus",
            table: "forecast_records"
        );

        migrationBuilder.DropColumn(
            name: "actual_volume",
            schema: "plutus",
            table: "forecast_records"
        );

        migrationBuilder.DropColumn(
            name: "evaluated_at",
            schema: "plutus",
            table: "forecast_records"
        );

        migrationBuilder.CreateIndex(
            name: "ix_forecast_records_symbol_id_target_at",
            schema: "plutus",
            table: "forecast_records",
            columns: new[] { "symbol_id", "target_at" }
        );

        migrationBuilder.Sql(
            """
            CREATE OR REPLACE VIEW plutus.forecast_records_with_actuals AS
            SELECT
                fr.id,
                fr.run_id,
                fr.market_id,
                fr.symbol_id,
                fr.model_name,
                fr.generated_at,
                fr.target_at,
                fr.horizon_days,
                fr.predicted_average_price,
                fr.predicted_min_price,
                fr.predicted_max_price,
                fr.predicted_volume,
                fr.created_at,
                fr.updated_at,
                d.total_spent / NULLIF(d.volume, 0) AS actual_average_price,
                d.min_price                         AS actual_min_price,
                d.max_price                         AS actual_max_price,
                d.volume                            AS actual_volume
            FROM plutus.forecast_records fr
            LEFT JOIN plutus.trades_daily d
                ON d.symbol_id = fr.symbol_id
               AND d.bucket = date_trunc('day', fr.target_at);
            """
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DROP VIEW IF EXISTS plutus.forecast_records_with_actuals;
            """
        );

        migrationBuilder.DropIndex(
            name: "ix_forecast_records_symbol_id_target_at",
            schema: "plutus",
            table: "forecast_records"
        );

        migrationBuilder.AddColumn<decimal>(
            name: "actual_average_price",
            schema: "plutus",
            table: "forecast_records",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );

        migrationBuilder.AddColumn<decimal>(
            name: "actual_max_price",
            schema: "plutus",
            table: "forecast_records",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );

        migrationBuilder.AddColumn<decimal>(
            name: "actual_min_price",
            schema: "plutus",
            table: "forecast_records",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );

        migrationBuilder.AddColumn<decimal>(
            name: "actual_volume",
            schema: "plutus",
            table: "forecast_records",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "evaluated_at",
            schema: "plutus",
            table: "forecast_records",
            type: "timestamp with time zone",
            nullable: true
        );

        migrationBuilder.CreateIndex(
            name: "ix_forecast_records_target_at_evaluated_at",
            schema: "plutus",
            table: "forecast_records",
            columns: new[] { "target_at", "evaluated_at" }
        );
    }
}

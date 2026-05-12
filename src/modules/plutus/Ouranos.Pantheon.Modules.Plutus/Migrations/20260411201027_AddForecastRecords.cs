using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Migrations;

/// <inheritdoc />
public partial class AddForecastRecords : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "forecast_runs",
            schema: "plutus",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                model_name = table.Column<string>(type: "text", nullable: false),
                generated_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false
                ),
                created_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false
                ),
                updated_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false
                ),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_forecast_runs", x => x.id);
            }
        );

        migrationBuilder.CreateTable(
            name: "forecast_records",
            schema: "plutus",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                run_id = table.Column<Guid>(type: "uuid", nullable: false),
                market_id = table.Column<Guid>(type: "uuid", nullable: false),
                symbol_id = table.Column<Guid>(type: "uuid", nullable: false),
                model_name = table.Column<string>(type: "text", nullable: false),
                generated_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false
                ),
                target_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false
                ),
                horizon_days = table.Column<int>(type: "integer", nullable: false),
                predicted_average_price = table.Column<decimal>(
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: false
                ),
                predicted_min_price = table.Column<decimal>(
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: false
                ),
                predicted_max_price = table.Column<decimal>(
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: false
                ),
                predicted_volume = table.Column<decimal>(
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: false
                ),
                actual_average_price = table.Column<decimal>(
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: true
                ),
                actual_min_price = table.Column<decimal>(
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: true
                ),
                actual_max_price = table.Column<decimal>(
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: true
                ),
                actual_volume = table.Column<decimal>(
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: true
                ),
                evaluated_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: true
                ),
                created_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false
                ),
                updated_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false
                ),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_forecast_records", x => x.id);
                table.ForeignKey(
                    name: "fk_forecast_records_forecast_runs_run_id",
                    column: x => x.run_id,
                    principalSchema: "plutus",
                    principalTable: "forecast_runs",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade
                );
                table.ForeignKey(
                    name: "fk_forecast_records_symbols_symbol_id",
                    column: x => x.symbol_id,
                    principalSchema: "plutus",
                    principalTable: "symbols",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateIndex(
            name: "ix_forecast_records_run_id",
            schema: "plutus",
            table: "forecast_records",
            column: "run_id"
        );

        migrationBuilder.CreateIndex(
            name: "ix_forecast_records_symbol_id_generated_at",
            schema: "plutus",
            table: "forecast_records",
            columns: new[] { "symbol_id", "generated_at" }
        );

        migrationBuilder.CreateIndex(
            name: "ix_forecast_records_target_at_evaluated_at",
            schema: "plutus",
            table: "forecast_records",
            columns: new[] { "target_at", "evaluated_at" }
        );

        migrationBuilder.CreateIndex(
            name: "ix_forecast_runs_generated_at",
            schema: "plutus",
            table: "forecast_runs",
            column: "generated_at"
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "forecast_records", schema: "plutus");

        migrationBuilder.DropTable(name: "forecast_runs", schema: "plutus");
    }
}

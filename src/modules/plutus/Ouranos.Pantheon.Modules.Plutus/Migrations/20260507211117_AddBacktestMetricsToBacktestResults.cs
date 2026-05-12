using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Migrations;

/// <inheritdoc />
public partial class AddBacktestMetricsToBacktestResults : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "results_cagr",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );

        migrationBuilder.AddColumn<decimal>(
            name: "results_calmar_ratio",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );

        migrationBuilder.AddColumn<decimal>(
            name: "results_expectancy",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );

        migrationBuilder.AddColumn<decimal>(
            name: "results_profit_factor",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );

        migrationBuilder.AddColumn<decimal>(
            name: "results_sortino_ratio",
            schema: "plutus",
            table: "backtests",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "results_cagr", schema: "plutus", table: "backtests");

        migrationBuilder.DropColumn(
            name: "results_calmar_ratio",
            schema: "plutus",
            table: "backtests"
        );

        migrationBuilder.DropColumn(
            name: "results_expectancy",
            schema: "plutus",
            table: "backtests"
        );

        migrationBuilder.DropColumn(
            name: "results_profit_factor",
            schema: "plutus",
            table: "backtests"
        );

        migrationBuilder.DropColumn(
            name: "results_sortino_ratio",
            schema: "plutus",
            table: "backtests"
        );
    }
}

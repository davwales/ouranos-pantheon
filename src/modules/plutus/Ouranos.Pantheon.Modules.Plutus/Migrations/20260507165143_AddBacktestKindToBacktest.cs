using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Migrations;

/// <inheritdoc />
public partial class AddBacktestKindToBacktest : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "kind",
            schema: "plutus",
            table: "backtests",
            type: "text",
            nullable: false,
            defaultValue: "Backtest"
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "weight",
            schema: "plutus",
            table: "backtest_optimized_signal_weights",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,4)",
            oldPrecision: 18,
            oldScale: 4
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "weight",
            schema: "plutus",
            table: "backtest_optimized_components",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,4)",
            oldPrecision: 18,
            oldScale: 4
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "kind",
            schema: "plutus",
            table: "backtests"
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "weight",
            schema: "plutus",
            table: "backtest_optimized_signal_weights",
            type: "numeric(18,4)",
            precision: 18,
            scale: 4,
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2
        );

        migrationBuilder.AlterColumn<decimal>(
            name: "weight",
            schema: "plutus",
            table: "backtest_optimized_components",
            type: "numeric(18,4)",
            precision: 18,
            scale: 4,
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2
        );
    }
}

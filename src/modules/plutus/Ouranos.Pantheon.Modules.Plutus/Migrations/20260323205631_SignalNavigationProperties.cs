using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Migrations;

/// <inheritdoc />
public partial class SignalNavigationProperties : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddForeignKey(
            name: "fk_signals_markets_market_id",
            schema: "plutus",
            table: "signals",
            column: "market_id",
            principalSchema: "plutus",
            principalTable: "markets",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade
        );

        migrationBuilder.AddForeignKey(
            name: "fk_signals_symbols_symbol_id",
            schema: "plutus",
            table: "signals",
            column: "symbol_id",
            principalSchema: "plutus",
            principalTable: "symbols",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_signals_markets_market_id",
            schema: "plutus",
            table: "signals"
        );

        migrationBuilder.DropForeignKey(
            name: "fk_signals_symbols_symbol_id",
            schema: "plutus",
            table: "signals"
        );
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Migrations;

/// <inheritdoc />
public partial class AddSignalComputedAtIndex : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "ix_signals_computed_at",
            schema: "plutus",
            table: "signals",
            column: "computed_at",
            descending: new[] { true }
        );

        migrationBuilder.CreateIndex(
            name: "ix_signals_symbol_id_computed_at",
            schema: "plutus",
            table: "signals",
            columns: new[] { "symbol_id", "computed_at" },
            descending: new[] { false, true }
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_signals_computed_at",
            schema: "plutus",
            table: "signals"
        );

        migrationBuilder.DropIndex(
            name: "ix_signals_symbol_id_computed_at",
            schema: "plutus",
            table: "signals"
        );
    }
}

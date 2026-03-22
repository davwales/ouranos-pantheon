using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Migrations;

/// <inheritdoc />
public partial class AddSignals : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "signals",
            schema: "plutus",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                market_id = table.Column<Guid>(type: "uuid", nullable: false),
                symbol_id = table.Column<Guid>(type: "uuid", nullable: false),
                type = table.Column<int>(type: "integer", nullable: false),
                value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                computed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => { table.PrimaryKey("pk_signals", x => x.id); }
        );

        migrationBuilder.CreateIndex(
            name: "ix_signals_market_id",
            schema: "plutus",
            table: "signals",
            column: "market_id"
        );

        migrationBuilder.CreateIndex(
            name: "ix_signals_symbol_id_type",
            schema: "plutus",
            table: "signals",
            columns: new[] { "symbol_id", "type" }
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "signals",
            schema: "plutus"
        );
    }
}

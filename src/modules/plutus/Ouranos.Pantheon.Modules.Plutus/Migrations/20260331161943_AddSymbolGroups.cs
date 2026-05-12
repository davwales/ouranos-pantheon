using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Migrations;

/// <inheritdoc />
public partial class AddSymbolGroups : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "symbol_groups",
            schema: "plutus",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                description = table.Column<string>(type: "text", nullable: true),
                market_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                table.PrimaryKey("pk_symbol_groups", x => x.id);
                table.ForeignKey(
                    name: "fk_symbol_groups_markets_market_id",
                    column: x => x.market_id,
                    principalSchema: "plutus",
                    principalTable: "markets",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateTable(
            name: "symbol_group_members",
            schema: "plutus",
            columns: table => new
            {
                symbol_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                symbol_id = table.Column<Guid>(type: "uuid", nullable: false),
                added_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false
                ),
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "pk_symbol_group_members",
                    x => new { x.symbol_group_id, x.symbol_id }
                );
                table.ForeignKey(
                    name: "fk_symbol_group_members_symbol_groups_symbol_group_id",
                    column: x => x.symbol_group_id,
                    principalSchema: "plutus",
                    principalTable: "symbol_groups",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade
                );
                table.ForeignKey(
                    name: "fk_symbol_group_members_symbols_symbol_id",
                    column: x => x.symbol_id,
                    principalSchema: "plutus",
                    principalTable: "symbols",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateIndex(
            name: "ix_symbol_group_members_symbol_id",
            schema: "plutus",
            table: "symbol_group_members",
            column: "symbol_id"
        );

        migrationBuilder.CreateIndex(
            name: "ix_symbol_groups_market_id",
            schema: "plutus",
            table: "symbol_groups",
            column: "market_id"
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "symbol_group_members", schema: "plutus");

        migrationBuilder.DropTable(name: "symbol_groups", schema: "plutus");
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Migrations;

/// <inheritdoc />
public partial class AddPositions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "positions",
            schema: "plutus",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                side = table.Column<int>(type: "integer", nullable: false),
                status = table.Column<int>(type: "integer", nullable: false),
                market_id = table.Column<Guid>(type: "uuid", nullable: false),
                symbol_id = table.Column<Guid>(type: "uuid", nullable: false),
                cost = table.Column<decimal>(
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: false
                ),
                quantity = table.Column<decimal>(
                    type: "numeric(18,4)",
                    precision: 18,
                    scale: 4,
                    nullable: false
                ),
                linked_buy_position_id = table.Column<Guid>(type: "uuid", nullable: true),
                strategy_id = table.Column<Guid>(type: "uuid", nullable: true),
                notes = table.Column<string>(
                    type: "character varying(2000)",
                    maxLength: 2000,
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
                table.PrimaryKey("pk_positions", x => x.id);
                table.ForeignKey(
                    name: "fk_positions_positions_linked_buy_position_id",
                    column: x => x.linked_buy_position_id,
                    principalSchema: "plutus",
                    principalTable: "positions",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict
                );
                table.ForeignKey(
                    name: "fk_positions_symbols_symbol_id",
                    column: x => x.symbol_id,
                    principalSchema: "plutus",
                    principalTable: "symbols",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict
                );
            }
        );

        migrationBuilder.CreateIndex(
            name: "ix_positions_linked_buy_position_id",
            schema: "plutus",
            table: "positions",
            column: "linked_buy_position_id"
        );

        migrationBuilder.CreateIndex(
            name: "ix_positions_market_id",
            schema: "plutus",
            table: "positions",
            column: "market_id"
        );

        migrationBuilder.CreateIndex(
            name: "ix_positions_market_id_status",
            schema: "plutus",
            table: "positions",
            columns: new[] { "market_id", "status" }
        );

        migrationBuilder.CreateIndex(
            name: "ix_positions_symbol_id",
            schema: "plutus",
            table: "positions",
            column: "symbol_id"
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "positions", schema: "plutus");
    }
}

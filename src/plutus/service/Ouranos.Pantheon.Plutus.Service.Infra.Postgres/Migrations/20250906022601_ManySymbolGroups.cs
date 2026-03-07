using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Plutus.Service.Infra.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class ManySymbolGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_symbols_symbol_groups_symbol_group_id",
                schema: "plutus",
                table: "symbols");

            migrationBuilder.DropIndex(
                name: "ix_symbols_symbol_group_id",
                schema: "plutus",
                table: "symbols");

            migrationBuilder.DropColumn(
                name: "symbol_group_id",
                schema: "plutus",
                table: "symbols");

            migrationBuilder.CreateTable(
                name: "symbol_symbol_group",
                schema: "plutus",
                columns: table => new
                {
                    symbol_groups_id = table.Column<Guid>(type: "uuid", nullable: false),
                    symbols_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_symbol_symbol_group", x => new { x.symbol_groups_id, x.symbols_id });
                    table.ForeignKey(
                        name: "fk_symbol_symbol_group_symbol_groups_symbol_groups_id",
                        column: x => x.symbol_groups_id,
                        principalSchema: "plutus",
                        principalTable: "symbol_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_symbol_symbol_group_symbols_symbols_id",
                        column: x => x.symbols_id,
                        principalSchema: "plutus",
                        principalTable: "symbols",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_symbol_symbol_group_symbols_id",
                schema: "plutus",
                table: "symbol_symbol_group",
                column: "symbols_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "symbol_symbol_group",
                schema: "plutus");

            migrationBuilder.AddColumn<Guid>(
                name: "symbol_group_id",
                schema: "plutus",
                table: "symbols",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_symbols_symbol_group_id",
                schema: "plutus",
                table: "symbols",
                column: "symbol_group_id");

            migrationBuilder.AddForeignKey(
                name: "fk_symbols_symbol_groups_symbol_group_id",
                schema: "plutus",
                table: "symbols",
                column: "symbol_group_id",
                principalSchema: "plutus",
                principalTable: "symbol_groups",
                principalColumn: "id");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Plutus.Service.Infra.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class RemovedDuplicateFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "symbol_name",
                schema: "plutus",
                table: "forecasts");

            migrationBuilder.DropColumn(
                name: "symbol_subcode",
                schema: "plutus",
                table: "forecasts");

            migrationBuilder.AddColumn<Guid>(
                name: "symbol_group_id",
                schema: "plutus",
                table: "symbols",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_symbols_market_id",
                schema: "plutus",
                table: "symbols",
                column: "market_id");

            migrationBuilder.CreateIndex(
                name: "ix_symbols_symbol_group_id",
                schema: "plutus",
                table: "symbols",
                column: "symbol_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_symbol_groups_market_id",
                schema: "plutus",
                table: "symbol_groups",
                column: "market_id");

            migrationBuilder.CreateIndex(
                name: "ix_recipes_market_id",
                schema: "plutus",
                table: "recipes",
                column: "market_id");

            migrationBuilder.CreateIndex(
                name: "ix_forecasts_symbol_id",
                schema: "plutus",
                table: "forecasts",
                column: "symbol_id");

            migrationBuilder.AddForeignKey(
                name: "fk_forecasts_symbols_symbol_id",
                schema: "plutus",
                table: "forecasts",
                column: "symbol_id",
                principalSchema: "plutus",
                principalTable: "symbols",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_recipes_markets_market_id",
                schema: "plutus",
                table: "recipes",
                column: "market_id",
                principalSchema: "plutus",
                principalTable: "markets",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_symbol_groups_markets_market_id",
                schema: "plutus",
                table: "symbol_groups",
                column: "market_id",
                principalSchema: "plutus",
                principalTable: "markets",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_symbols_markets_market_id",
                schema: "plutus",
                table: "symbols",
                column: "market_id",
                principalSchema: "plutus",
                principalTable: "markets",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_symbols_symbol_groups_symbol_group_id",
                schema: "plutus",
                table: "symbols",
                column: "symbol_group_id",
                principalSchema: "plutus",
                principalTable: "symbol_groups",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_forecasts_symbols_symbol_id",
                schema: "plutus",
                table: "forecasts");

            migrationBuilder.DropForeignKey(
                name: "fk_recipes_markets_market_id",
                schema: "plutus",
                table: "recipes");

            migrationBuilder.DropForeignKey(
                name: "fk_symbol_groups_markets_market_id",
                schema: "plutus",
                table: "symbol_groups");

            migrationBuilder.DropForeignKey(
                name: "fk_symbols_markets_market_id",
                schema: "plutus",
                table: "symbols");

            migrationBuilder.DropForeignKey(
                name: "fk_symbols_symbol_groups_symbol_group_id",
                schema: "plutus",
                table: "symbols");

            migrationBuilder.DropIndex(
                name: "ix_symbols_market_id",
                schema: "plutus",
                table: "symbols");

            migrationBuilder.DropIndex(
                name: "ix_symbols_symbol_group_id",
                schema: "plutus",
                table: "symbols");

            migrationBuilder.DropIndex(
                name: "ix_symbol_groups_market_id",
                schema: "plutus",
                table: "symbol_groups");

            migrationBuilder.DropIndex(
                name: "ix_recipes_market_id",
                schema: "plutus",
                table: "recipes");

            migrationBuilder.DropIndex(
                name: "ix_forecasts_symbol_id",
                schema: "plutus",
                table: "forecasts");

            migrationBuilder.DropColumn(
                name: "symbol_group_id",
                schema: "plutus",
                table: "symbols");

            migrationBuilder.AddColumn<string>(
                name: "symbol_name",
                schema: "plutus",
                table: "forecasts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "symbol_subcode",
                schema: "plutus",
                table: "forecasts",
                type: "text",
                nullable: true);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Migrations;

/// <inheritdoc />
public partial class AddMarketTradeSnapshots : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "market_trade_snapshots",
            schema: "plutus",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                market_id = table.Column<Guid>(type: "uuid", nullable: false),
                symbol_id = table.Column<Guid>(type: "uuid", nullable: false),
                time_frame = table.Column<string>(type: "text", nullable: false),
                total_spent = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                min_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                max_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                total_volume = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                num_transactions = table.Column<int>(type: "integer", nullable: false),
                limit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                tax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_market_trade_snapshots", x => x.id);
                table.ForeignKey(
                    name: "fk_market_trade_snapshots_markets_market_id",
                    column: x => x.market_id,
                    principalSchema: "plutus",
                    principalTable: "markets",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_market_trade_snapshots_symbols_symbol_id",
                    column: x => x.symbol_id,
                    principalSchema: "plutus",
                    principalTable: "symbols",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_market_trade_snapshots_market_id_time_frame",
            schema: "plutus",
            table: "market_trade_snapshots",
            columns: new[] { "market_id", "time_frame" });

        migrationBuilder.CreateIndex(
            name: "ix_market_trade_snapshots_symbol_id_time_frame",
            schema: "plutus",
            table: "market_trade_snapshots",
            columns: new[] { "symbol_id", "time_frame" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "market_trade_snapshots",
            schema: "plutus");
    }
}

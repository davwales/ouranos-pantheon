using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Migrations;

/// <inheritdoc />
public partial class AddMarketOverviewBuckets : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "market_overview_buckets",
            schema: "plutus",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                market_id = table.Column<Guid>(type: "uuid", nullable: false),
                time_frame = table.Column<string>(type: "text", nullable: false),
                bucket_start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                average_price =
                    table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                min_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                max_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                volume = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                total_spent =
                    table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                num_transactions = table.Column<int>(type: "integer", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_market_overview_buckets", x => x.id);
                table.ForeignKey(
                    name: "fk_market_overview_buckets_markets_market_id",
                    column: x => x.market_id,
                    principalSchema: "plutus",
                    principalTable: "markets",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateIndex(
            name: "ix_market_overview_buckets_market_id_time_frame",
            schema: "plutus",
            table: "market_overview_buckets",
            columns: new[] { "market_id", "time_frame" }
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "market_overview_buckets",
            schema: "plutus"
        );
    }
}

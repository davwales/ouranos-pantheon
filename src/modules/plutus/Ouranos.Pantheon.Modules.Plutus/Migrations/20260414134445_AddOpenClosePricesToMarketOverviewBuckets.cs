using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Migrations;

/// <inheritdoc />
public partial class AddOpenClosePricesToMarketOverviewBuckets : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "close_price",
            schema: "plutus",
            table: "market_overview_buckets",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: false,
            defaultValue: 0m
        );

        migrationBuilder.AddColumn<decimal>(
            name: "open_price",
            schema: "plutus",
            table: "market_overview_buckets",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: false,
            defaultValue: 0m
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "close_price",
            schema: "plutus",
            table: "market_overview_buckets"
        );

        migrationBuilder.DropColumn(
            name: "open_price",
            schema: "plutus",
            table: "market_overview_buckets"
        );
    }
}

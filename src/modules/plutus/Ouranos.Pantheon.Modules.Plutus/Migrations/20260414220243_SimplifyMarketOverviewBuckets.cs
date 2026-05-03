using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Migrations;

/// <inheritdoc />
public partial class SimplifyMarketOverviewBuckets : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "close_price",
            schema: "plutus",
            table: "market_overview_buckets"
        );

        migrationBuilder.DropColumn(
            name: "max_price",
            schema: "plutus",
            table: "market_overview_buckets"
        );

        migrationBuilder.DropColumn(
            name: "min_price",
            schema: "plutus",
            table: "market_overview_buckets"
        );

        migrationBuilder.DropColumn(
            name: "open_price",
            schema: "plutus",
            table: "market_overview_buckets"
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
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
            name: "max_price",
            schema: "plutus",
            table: "market_overview_buckets",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: false,
            defaultValue: 0m
        );

        migrationBuilder.AddColumn<decimal>(
            name: "min_price",
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
}

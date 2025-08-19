using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Plutus.Service.Infra.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class SeedMarkets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "markets",
                columns: [
                    "id", 
                    "name", 
                    "description", 
                    "icon", 
                    "is_forecasting_enabled", 
                    "taxes_flat_maximum", 
                    "taxes_flat_minimum", 
                    "taxes_flat_rate",
                    "created_at",
                    "updated_at"
                ],
                values: new object[,]
                {
                    {
                        Guid.Parse("d71d7207-e30b-404f-8797-0148ad88cf9e"),
                        "OSRS",
                        "Explore market data from the game Old School RuneScape. Data is processed every 5 minutes by polling the Old School Wiki Market API.",
                        "https://oldschool.runescape.wiki/images/Old_School_RuneScape_Mobile_icon.png",
                        true,
                        1m,
                        5000000m,
                        0.02m,
                        DateTimeOffset.UtcNow,
                        DateTimeOffset.UtcNow
                    },
                    {
                        Guid.Parse("411b954f-5834-462e-9887-26d3ad76c924"),
                        "FFXIV",
                        "Explore market data from the game Final Fantasy XIV. Data is processed as it becomes available from the Universalis WebSocket API.",
                        "https://static.wikia.nocookie.net/ffxiv_gamepedia/images/e/e6/Site-logo.png",
                        false,
                        null,
                        null,
                        null,
                        DateTimeOffset.UtcNow,
                        DateTimeOffset.UtcNow
                    },
                    {
                        Guid.Parse("daebf0a1-b54d-44f4-9c21-6654c505169a"),
                        "Stock Market",
                        "Explore market data from the real life stock market. Data is processed as it becomes available from the Alpaca WebSocket API.",
                        "https://cdn-icons-png.flaticon.com/512/6410/6410570.png",
                        false,
                        null,
                        null,
                        null,
                        DateTimeOffset.UtcNow,
                        DateTimeOffset.UtcNow
                    },
                },
                schema: "plutus"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "markets",
                keyColumn: "id",
                keyValue: Guid.Parse("d71d7207-e30b-404f-8797-0148ad88cf9e"),
                schema: "plutus"
            );

            migrationBuilder.DeleteData(
                table: "markets",
                keyColumn: "id",
                keyValue: Guid.Parse("411b954f-5834-462e-9887-26d3ad76c924"),
                schema: "plutus"
            );

            migrationBuilder.DeleteData(
                table: "markets",
                keyColumn: "id",
                keyValue: Guid.Parse("daebf0a1-b54d-44f4-9c21-6654c505169a"),
                schema: "plutus"
            );
        }
    }
}

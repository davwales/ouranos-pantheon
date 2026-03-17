using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Database.Migrations;

/// <inheritdoc />
public partial class SeedMarkets : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            INSERT INTO plutus.markets (
                id, 
                name, 
                taxes_flat_minimum, 
                taxes_flat_maximum, 
                taxes_flat_rate, 
                is_forecasting_enabled, 
                description, 
                icon, 
                created_at, 
                updated_at
            ) VALUES 
            (
                '411b954f-5834-462e-9887-26d3ad76c924', 
                'FFXIV', 
                NULL, 
                NULL, 
                NULL, 
                FALSE, 
                'Explore market data from the game Final Fantasy XIV. Data is processed as it becomes available from the Universalis WebSocket API.', 
                'https://static.wikia.nocookie.net/ffxiv_gamepedia/images/e/e6/Site-logo.png', 
                '2026-03-12 19:19:51.595637+00', 
                '2026-03-12 19:19:51.595637+00'
            ),
            (
                'd71d7207-e30b-404f-8797-0148ad88cf9e', 
                'OSRS', 
                1.00, 
                5000000.00, 
                0.02, 
                TRUE, 
                'Explore market data from the game Old School RuneScape. Data is processed every 5 minutes by polling the Old School Wiki Market API.', 
                'https://oldschool.runescape.wiki/images/Old_School_RuneScape_Mobile_icon.png', 
                '2026-03-12 19:19:51.5956+00', 
                '2026-03-12 19:19:51.59562+00'
            ),
            (
                'daebf0a1-b54d-44f4-9c21-6654c505169a', 
                'Stock Market', 
                NULL, 
                NULL, 
                NULL, 
                FALSE, 
                'Explore market data from the real life stock market. Data is processed as it becomes available from the Alpaca WebSocket API.', 
                'https://cdn-icons-png.flaticon.com/512/6410/6410570.png', 
                '2026-03-12 19:19:51.595638+00', 
                '2026-03-12 19:19:51.595638+00'
            )
            ON CONFLICT (id) DO NOTHING;
            """
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DELETE FROM plutus.markets
            WHERE id in ('411b954f-5834-462e-9887-26d3ad76c924', 'd71d7207-e30b-404f-8797-0148ad88cf9e', 'daebf0a1-b54d-44f4-9c21-6654c505169a'
            """
        );
    }
}

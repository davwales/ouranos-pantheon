using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Migrations;

/// <inheritdoc />
public partial class AddBacktestProgressFields : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "progress_message",
            schema: "plutus",
            table: "backtests",
            type: "character varying(256)",
            maxLength: 256,
            nullable: true
        );

        migrationBuilder.AddColumn<int>(
            name: "progress_percent",
            schema: "plutus",
            table: "backtests",
            type: "integer",
            nullable: false,
            defaultValue: 0
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "progress_message",
            schema: "plutus",
            table: "backtests"
        );

        migrationBuilder.DropColumn(
            name: "progress_percent",
            schema: "plutus",
            table: "backtests"
        );
    }
}

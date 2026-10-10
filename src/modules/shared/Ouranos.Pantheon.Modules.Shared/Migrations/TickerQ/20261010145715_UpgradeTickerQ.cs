using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Shared.Migrations.TickerQ;

/// <inheritdoc />
public partial class UpgradeTickerQ : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<bool>(
            name: "is_enabled",
            schema: "ticker",
            table: "CronTickers",
            type: "boolean",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "boolean",
            oldDefaultValue: true
        );

        migrationBuilder.AddColumn<bool>(
            name: "is_system_paused",
            schema: "ticker",
            table: "CronTickers",
            type: "boolean",
            nullable: false,
            defaultValue: false
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "is_system_paused",
            schema: "ticker",
            table: "CronTickers"
        );

        migrationBuilder.AlterColumn<bool>(
            name: "is_enabled",
            schema: "ticker",
            table: "CronTickers",
            type: "boolean",
            nullable: false,
            defaultValue: true,
            oldClrType: typeof(bool),
            oldType: "boolean"
        );
    }
}

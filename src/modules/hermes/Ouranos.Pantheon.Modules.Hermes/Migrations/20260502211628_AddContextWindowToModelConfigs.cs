using Microsoft.EntityFrameworkCore.Migrations;

namespace Ouranos.Pantheon.Hermes.Service.Infra.Postgres.Migrations;

/// <inheritdoc />
public partial class AddContextWindowToModelConfigs : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "context_window",
            schema: "hermes",
            table: "model_configs",
            type: "integer",
            nullable: true
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "context_window",
            schema: "hermes",
            table: "model_configs"
        );
    }
}
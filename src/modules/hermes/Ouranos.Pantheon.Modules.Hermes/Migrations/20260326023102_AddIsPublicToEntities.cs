using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Hermes.Service.Infra.Postgres.Migrations;

/// <inheritdoc />
public partial class AddIsPublicToEntities : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "is_public",
            schema: "hermes",
            table: "traits",
            type: "boolean",
            nullable: false,
            defaultValue: true
        );

        migrationBuilder.AddColumn<bool>(
            name: "is_public",
            schema: "hermes",
            table: "personas",
            type: "boolean",
            nullable: false,
            defaultValue: true
        );

        migrationBuilder.AddColumn<bool>(
            name: "is_public",
            schema: "hermes",
            table: "model_configs",
            type: "boolean",
            nullable: false,
            defaultValue: true
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "is_public",
            schema: "hermes",
            table: "traits"
        );

        migrationBuilder.DropColumn(
            name: "is_public",
            schema: "hermes",
            table: "personas"
        );

        migrationBuilder.DropColumn(
            name: "is_public",
            schema: "hermes",
            table: "model_configs"
        );
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Hermes.Service.Infra.Postgres.Migrations;

/// <inheritdoc />
public partial class AddTokenUsageToConversations : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "input_token_count",
            schema: "hermes",
            table: "conversations",
            type: "integer",
            nullable: true
        );

        migrationBuilder.AddColumn<int>(
            name: "output_token_count",
            schema: "hermes",
            table: "conversations",
            type: "integer",
            nullable: true
        );

        migrationBuilder.AddColumn<int>(
            name: "total_token_count",
            schema: "hermes",
            table: "conversations",
            type: "integer",
            nullable: true
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "input_token_count",
            schema: "hermes",
            table: "conversations"
        );

        migrationBuilder.DropColumn(
            name: "output_token_count",
            schema: "hermes",
            table: "conversations"
        );

        migrationBuilder.DropColumn(
            name: "total_token_count",
            schema: "hermes",
            table: "conversations"
        );
    }
}

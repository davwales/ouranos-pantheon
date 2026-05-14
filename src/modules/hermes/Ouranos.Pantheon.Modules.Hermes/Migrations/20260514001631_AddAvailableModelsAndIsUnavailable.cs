using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Hermes.Migrations;

/// <inheritdoc />
public partial class AddAvailableModelsAndIsUnavailable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "is_unavailable",
            schema: "hermes",
            table: "model_configs",
            type: "boolean",
            nullable: false,
            defaultValue: false
        );

        migrationBuilder.CreateTable(
            name: "available_models",
            schema: "hermes",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                model_identifier = table.Column<string>(type: "text", nullable: false),
                owned_by = table.Column<string>(type: "text", nullable: false),
                created_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false
                ),
                updated_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false
                ),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_available_models", x => x.id);
            }
        );

        migrationBuilder.CreateIndex(
            name: "ix_available_models_model_identifier",
            schema: "hermes",
            table: "available_models",
            column: "model_identifier",
            unique: true
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "available_models", schema: "hermes");

        migrationBuilder.DropColumn(
            name: "is_unavailable",
            schema: "hermes",
            table: "model_configs"
        );
    }
}

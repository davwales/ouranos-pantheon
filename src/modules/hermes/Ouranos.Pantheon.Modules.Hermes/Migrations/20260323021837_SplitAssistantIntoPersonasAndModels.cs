using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Hermes.Service.Infra.Postgres.Migrations;

/// <inheritdoc />
public partial class SplitAssistantIntoPersonasAndModels : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "assistants", schema: "hermes");

        migrationBuilder.CreateTable(
            name: "model_configs",
            schema: "hermes",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                model_identifier = table.Column<string>(type: "text", nullable: false),
                system_prompt = table.Column<string>(type: "text", nullable: false),
                temperature = table.Column<float>(type: "real", nullable: true),
                max_tokens = table.Column<int>(type: "integer", nullable: true),
                repeat_penalty = table.Column<float>(type: "real", nullable: true),
                is_default = table.Column<bool>(type: "boolean", nullable: false),
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
                table.PrimaryKey("pk_model_configs", x => x.id);
            }
        );

        migrationBuilder.CreateTable(
            name: "personas",
            schema: "hermes",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                description = table.Column<string>(type: "text", nullable: false),
                personality = table.Column<string>(type: "text", nullable: true),
                scenario = table.Column<string>(type: "text", nullable: true),
                is_default = table.Column<bool>(type: "boolean", nullable: false),
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
                table.PrimaryKey("pk_personas", x => x.id);
            }
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "model_configs", schema: "hermes");

        migrationBuilder.DropTable(name: "personas", schema: "hermes");

        migrationBuilder.CreateTable(
            name: "assistants",
            schema: "hermes",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                assistant_name = table.Column<string>(type: "text", nullable: false),
                created_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false
                ),
                max_tokens = table.Column<int>(type: "integer", nullable: true),
                model = table.Column<string>(type: "text", nullable: false),
                repeat_penalty = table.Column<float>(type: "real", nullable: true),
                system_prompt = table.Column<string>(type: "text", nullable: false),
                temperature = table.Column<float>(type: "real", nullable: true),
                updated_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false
                ),
                user_name = table.Column<string>(type: "text", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_assistants", x => x.id);
            }
        );
    }
}

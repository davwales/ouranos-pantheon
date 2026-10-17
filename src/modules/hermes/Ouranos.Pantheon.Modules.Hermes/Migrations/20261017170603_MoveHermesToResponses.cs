using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Hermes.Migrations;

/// <inheritdoc />
public partial class MoveHermesToResponses : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "repeat_penalty",
            schema: "hermes",
            table: "model_configs"
        );

        migrationBuilder.AddColumn<bool>(
            name: "is_tool_calling_enabled",
            schema: "hermes",
            table: "model_configs",
            type: "boolean",
            nullable: false,
            defaultValue: false
        );

        migrationBuilder.AddColumn<string>(
            name: "reasoning_effort",
            schema: "hermes",
            table: "conversations",
            type: "text",
            nullable: true
        );

        migrationBuilder.CreateTable(
            name: "message_parts",
            schema: "hermes",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                message_id = table.Column<Guid>(type: "uuid", nullable: false),
                sort_order = table.Column<int>(type: "integer", nullable: false),
                kind = table.Column<string>(
                    type: "character varying(13)",
                    maxLength: 13,
                    nullable: false
                ),
                content = table.Column<string>(type: "text", nullable: true),
                call_id = table.Column<string>(type: "text", nullable: true),
                name = table.Column<string>(type: "text", nullable: true),
                arguments = table.Column<string>(type: "text", nullable: true),
                status = table.Column<string>(type: "text", nullable: true),
                output = table.Column<string>(type: "text", nullable: true),
                is_truncated = table.Column<bool>(type: "boolean", nullable: true),
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
                table.PrimaryKey("pk_message_parts", x => x.id);
                table.ForeignKey(
                    name: "fk_message_parts_messages_message_id",
                    column: x => x.message_id,
                    principalSchema: "hermes",
                    principalTable: "messages",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateIndex(
            name: "ix_message_parts_message_id_sort_order",
            schema: "hermes",
            table: "message_parts",
            columns: new[] { "message_id", "sort_order" }
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "message_parts", schema: "hermes");

        migrationBuilder.DropColumn(
            name: "is_tool_calling_enabled",
            schema: "hermes",
            table: "model_configs"
        );

        migrationBuilder.DropColumn(
            name: "reasoning_effort",
            schema: "hermes",
            table: "conversations"
        );

        migrationBuilder.AddColumn<float>(
            name: "repeat_penalty",
            schema: "hermes",
            table: "model_configs",
            type: "real",
            nullable: true
        );
    }
}

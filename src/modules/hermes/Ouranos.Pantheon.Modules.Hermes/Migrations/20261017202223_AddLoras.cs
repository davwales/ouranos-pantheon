using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Hermes.Migrations;

/// <inheritdoc />
public partial class AddLoras : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "loras",
            schema: "hermes",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                identifier = table.Column<string>(type: "text", nullable: false),
                weight = table.Column<float>(type: "real", nullable: false),
                is_public = table.Column<bool>(type: "boolean", nullable: false),
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
                table.PrimaryKey("pk_loras", x => x.id);
            }
        );

        migrationBuilder.CreateTable(
            name: "conversation_lora",
            schema: "hermes",
            columns: table => new
            {
                conversation_id = table.Column<Guid>(type: "uuid", nullable: false),
                loras_id = table.Column<Guid>(type: "uuid", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "pk_conversation_lora",
                    x => new { x.conversation_id, x.loras_id }
                );
                table.ForeignKey(
                    name: "fk_conversation_lora_conversations_conversation_id",
                    column: x => x.conversation_id,
                    principalSchema: "hermes",
                    principalTable: "conversations",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade
                );
                table.ForeignKey(
                    name: "fk_conversation_lora_loras_loras_id",
                    column: x => x.loras_id,
                    principalSchema: "hermes",
                    principalTable: "loras",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateIndex(
            name: "ix_conversation_lora_loras_id",
            schema: "hermes",
            table: "conversation_lora",
            column: "loras_id"
        );

        migrationBuilder.CreateIndex(
            name: "ix_loras_identifier",
            schema: "hermes",
            table: "loras",
            column: "identifier",
            unique: true
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "conversation_lora", schema: "hermes");

        migrationBuilder.DropTable(name: "loras", schema: "hermes");
    }
}

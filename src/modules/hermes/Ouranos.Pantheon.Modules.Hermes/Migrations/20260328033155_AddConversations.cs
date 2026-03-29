using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Hermes.Service.Infra.Postgres.Migrations;

/// <inheritdoc />
public partial class AddConversations : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "conversations",
            schema: "hermes",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                persona_id = table.Column<Guid>(type: "uuid", nullable: false),
                model_config_id = table.Column<Guid>(type: "uuid", nullable: false),
                is_public = table.Column<bool>(type: "boolean", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_conversations", x => x.id);
                table.ForeignKey(
                    name: "fk_conversations_model_configs_model_config_id",
                    column: x => x.model_config_id,
                    principalSchema: "hermes",
                    principalTable: "model_configs",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict
                );
                table.ForeignKey(
                    name: "fk_conversations_personas_persona_id",
                    column: x => x.persona_id,
                    principalSchema: "hermes",
                    principalTable: "personas",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict
                );
            }
        );

        migrationBuilder.CreateTable(
            name: "conversation_trait",
            schema: "hermes",
            columns: table => new
            {
                conversation_id = table.Column<Guid>(type: "uuid", nullable: false),
                traits_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_conversation_trait", x => new { x.conversation_id, x.traits_id });
                table.ForeignKey(
                    name: "fk_conversation_trait_conversations_conversation_id",
                    column: x => x.conversation_id,
                    principalSchema: "hermes",
                    principalTable: "conversations",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade
                );
                table.ForeignKey(
                    name: "fk_conversation_trait_traits_traits_id",
                    column: x => x.traits_id,
                    principalSchema: "hermes",
                    principalTable: "traits",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateTable(
            name: "messages",
            schema: "hermes",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                conversation_id = table.Column<Guid>(type: "uuid", nullable: false),
                content = table.Column<string>(type: "text", nullable: false),
                role = table.Column<string>(type: "text", nullable: false),
                sort_order = table.Column<int>(type: "integer", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_messages", x => x.id);
                table.ForeignKey(
                    name: "fk_messages_conversations_conversation_id",
                    column: x => x.conversation_id,
                    principalSchema: "hermes",
                    principalTable: "conversations",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateIndex(
            name: "ix_conversation_trait_traits_id",
            schema: "hermes",
            table: "conversation_trait",
            column: "traits_id"
        );

        migrationBuilder.CreateIndex(
            name: "ix_conversations_model_config_id",
            schema: "hermes",
            table: "conversations",
            column: "model_config_id"
        );

        migrationBuilder.CreateIndex(
            name: "ix_conversations_persona_id",
            schema: "hermes",
            table: "conversations",
            column: "persona_id"
        );

        migrationBuilder.CreateIndex(
            name: "ix_messages_conversation_id_sort_order",
            schema: "hermes",
            table: "messages",
            columns: new[] { "conversation_id", "sort_order" }
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "conversation_trait",
            schema: "hermes"
        );

        migrationBuilder.DropTable(
            name: "messages",
            schema: "hermes"
        );

        migrationBuilder.DropTable(
            name: "conversations",
            schema: "hermes"
        );
    }
}

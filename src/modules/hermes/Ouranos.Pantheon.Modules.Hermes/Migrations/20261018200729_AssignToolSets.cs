using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Hermes.Migrations;

/// <inheritdoc />
public partial class AssignToolSets : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "default_tool_set_id",
            schema: "hermes",
            table: "model_configs",
            type: "uuid",
            nullable: true
        );

        migrationBuilder.AddColumn<Guid>(
            name: "tool_set_id",
            schema: "hermes",
            table: "conversations",
            type: "uuid",
            nullable: true
        );

        migrationBuilder.CreateIndex(
            name: "ix_model_configs_default_tool_set_id",
            schema: "hermes",
            table: "model_configs",
            column: "default_tool_set_id"
        );

        migrationBuilder.CreateIndex(
            name: "ix_conversations_tool_set_id",
            schema: "hermes",
            table: "conversations",
            column: "tool_set_id"
        );

        migrationBuilder.AddForeignKey(
            name: "fk_conversations_tool_sets_tool_set_id",
            schema: "hermes",
            table: "conversations",
            column: "tool_set_id",
            principalSchema: "hermes",
            principalTable: "tool_sets",
            principalColumn: "id",
            onDelete: ReferentialAction.SetNull
        );

        migrationBuilder.AddForeignKey(
            name: "fk_model_configs_tool_sets_default_tool_set_id",
            schema: "hermes",
            table: "model_configs",
            column: "default_tool_set_id",
            principalSchema: "hermes",
            principalTable: "tool_sets",
            principalColumn: "id",
            onDelete: ReferentialAction.SetNull
        );

        migrationBuilder.Sql(
            """
            DO $$
            DECLARE
                default_set_id uuid;
            BEGIN
                IF EXISTS (SELECT 1 FROM hermes.model_configs WHERE is_tool_calling_enabled) THEN
                    default_set_id := gen_random_uuid();

                    INSERT INTO hermes.tool_sets (id, name, description, instructions, is_public, created_at, updated_at)
                    VALUES (
                        default_set_id,
                        'Default',
                        'Created from the models that had tool calling enabled.',
                        'Call a tool whenever it helps the reply, without asking the user first.',
                        true,
                        now(),
                        now()
                    );

                    INSERT INTO hermes.tool_set_tools (id, tool_set_id, source, name, additional_description, created_at, updated_at)
                    VALUES (
                        gen_random_uuid(),
                        default_set_id,
                        'Native',
                        'generate_image',
                        'Call it on your own whenever a picture adds to the reply, or when the user asks for one.',
                        now(),
                        now()
                    );

                    UPDATE hermes.conversations c
                    SET tool_set_id = default_set_id
                    FROM hermes.model_configs m
                    WHERE c.model_config_id = m.id AND m.is_tool_calling_enabled;

                    UPDATE hermes.model_configs
                    SET default_tool_set_id = default_set_id
                    WHERE is_tool_calling_enabled;
                END IF;
            END $$;
            """
        );

        migrationBuilder.DropColumn(
            name: "is_tool_calling_enabled",
            schema: "hermes",
            table: "model_configs"
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "is_tool_calling_enabled",
            schema: "hermes",
            table: "model_configs",
            type: "boolean",
            nullable: false,
            defaultValue: false
        );

        migrationBuilder.Sql(
            "UPDATE hermes.model_configs SET is_tool_calling_enabled = true WHERE default_tool_set_id IS NOT NULL;"
        );

        migrationBuilder.DropForeignKey(
            name: "fk_conversations_tool_sets_tool_set_id",
            schema: "hermes",
            table: "conversations"
        );

        migrationBuilder.DropForeignKey(
            name: "fk_model_configs_tool_sets_default_tool_set_id",
            schema: "hermes",
            table: "model_configs"
        );

        migrationBuilder.DropIndex(
            name: "ix_model_configs_default_tool_set_id",
            schema: "hermes",
            table: "model_configs"
        );

        migrationBuilder.DropIndex(
            name: "ix_conversations_tool_set_id",
            schema: "hermes",
            table: "conversations"
        );

        migrationBuilder.DropColumn(
            name: "default_tool_set_id",
            schema: "hermes",
            table: "model_configs"
        );

        migrationBuilder.DropColumn(name: "tool_set_id", schema: "hermes", table: "conversations");
    }
}

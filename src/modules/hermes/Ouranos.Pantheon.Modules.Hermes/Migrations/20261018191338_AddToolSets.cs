using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Hermes.Migrations;

/// <inheritdoc />
public partial class AddToolSets : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "tool_sets",
            schema: "hermes",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                description = table.Column<string>(type: "text", nullable: true),
                instructions = table.Column<string>(
                    type: "character varying(1000)",
                    maxLength: 1000,
                    nullable: true
                ),
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
                table.PrimaryKey("pk_tool_sets", x => x.id);
            }
        );

        migrationBuilder.CreateTable(
            name: "tool_set_tools",
            schema: "hermes",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tool_set_id = table.Column<Guid>(type: "uuid", nullable: false),
                source = table.Column<string>(type: "text", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                additional_description = table.Column<string>(
                    type: "character varying(500)",
                    maxLength: 500,
                    nullable: true
                ),
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
                table.PrimaryKey("pk_tool_set_tools", x => x.id);
                table.ForeignKey(
                    name: "fk_tool_set_tools_tool_sets_tool_set_id",
                    column: x => x.tool_set_id,
                    principalSchema: "hermes",
                    principalTable: "tool_sets",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateIndex(
            name: "ix_tool_set_tools_tool_set_id_source_name",
            schema: "hermes",
            table: "tool_set_tools",
            columns: new[] { "tool_set_id", "source", "name" },
            unique: true
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "tool_set_tools", schema: "hermes");

        migrationBuilder.DropTable(name: "tool_sets", schema: "hermes");
    }
}

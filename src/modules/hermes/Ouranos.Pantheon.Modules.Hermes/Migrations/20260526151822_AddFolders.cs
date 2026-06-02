using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Hermes.Migrations;

/// <inheritdoc />
public partial class AddFolders : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "folder_id",
            schema: "hermes",
            table: "conversations",
            type: "uuid",
            nullable: true
        );

        migrationBuilder.CreateTable(
            name: "folders",
            schema: "hermes",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                parent_folder_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                table.PrimaryKey("pk_folders", x => x.id);
                table.ForeignKey(
                    name: "fk_folders_folders_parent_folder_id",
                    column: x => x.parent_folder_id,
                    principalSchema: "hermes",
                    principalTable: "folders",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateIndex(
            name: "ix_conversations_folder_id",
            schema: "hermes",
            table: "conversations",
            column: "folder_id"
        );

        migrationBuilder.CreateIndex(
            name: "ix_folders_parent_folder_id",
            schema: "hermes",
            table: "folders",
            column: "parent_folder_id"
        );

        migrationBuilder.AddForeignKey(
            name: "fk_conversations_folders_folder_id",
            schema: "hermes",
            table: "conversations",
            column: "folder_id",
            principalSchema: "hermes",
            principalTable: "folders",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_conversations_folders_folder_id",
            schema: "hermes",
            table: "conversations"
        );

        migrationBuilder.DropTable(name: "folders", schema: "hermes");

        migrationBuilder.DropIndex(
            name: "ix_conversations_folder_id",
            schema: "hermes",
            table: "conversations"
        );

        migrationBuilder.DropColumn(name: "folder_id", schema: "hermes", table: "conversations");
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Hermes.Migrations;

/// <inheritdoc />
public partial class AddGeneratedImages : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "alt_text",
            schema: "hermes",
            table: "message_parts",
            type: "text",
            nullable: true
        );

        migrationBuilder.AddColumn<long>(
            name: "duration_ms",
            schema: "hermes",
            table: "message_parts",
            type: "bigint",
            nullable: true
        );

        migrationBuilder.AddColumn<int>(
            name: "height",
            schema: "hermes",
            table: "message_parts",
            type: "integer",
            nullable: true
        );

        migrationBuilder.AddColumn<Guid>(
            name: "image_id",
            schema: "hermes",
            table: "message_parts",
            type: "uuid",
            nullable: true
        );

        migrationBuilder.AddColumn<int>(
            name: "width",
            schema: "hermes",
            table: "message_parts",
            type: "integer",
            nullable: true
        );

        migrationBuilder.CreateTable(
            name: "generated_images",
            schema: "hermes",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                content = table.Column<byte[]>(type: "bytea", nullable: false),
                media_type = table.Column<string>(type: "text", nullable: false),
                width = table.Column<int>(type: "integer", nullable: false),
                height = table.Column<int>(type: "integer", nullable: false),
                prompt = table.Column<string>(type: "text", nullable: false),
                model = table.Column<string>(type: "text", nullable: false),
                seed = table.Column<long>(type: "bigint", nullable: true),
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
                table.PrimaryKey("pk_generated_images", x => x.id);
            }
        );

        migrationBuilder.CreateIndex(
            name: "ix_message_parts_image_id",
            schema: "hermes",
            table: "message_parts",
            column: "image_id"
        );

        migrationBuilder.CreateIndex(
            name: "ix_generated_images_created_at",
            schema: "hermes",
            table: "generated_images",
            column: "created_at"
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "generated_images", schema: "hermes");

        migrationBuilder.DropIndex(
            name: "ix_message_parts_image_id",
            schema: "hermes",
            table: "message_parts"
        );

        migrationBuilder.DropColumn(name: "alt_text", schema: "hermes", table: "message_parts");

        migrationBuilder.DropColumn(name: "duration_ms", schema: "hermes", table: "message_parts");

        migrationBuilder.DropColumn(name: "height", schema: "hermes", table: "message_parts");

        migrationBuilder.DropColumn(name: "image_id", schema: "hermes", table: "message_parts");

        migrationBuilder.DropColumn(name: "width", schema: "hermes", table: "message_parts");
    }
}

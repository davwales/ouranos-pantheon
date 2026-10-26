using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Hermes.Migrations;

/// <inheritdoc />
public partial class AddGeneratedImageSource : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "source_image_id",
            schema: "hermes",
            table: "generated_images",
            type: "uuid",
            nullable: true
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "source_image_id",
            schema: "hermes",
            table: "generated_images"
        );
    }
}

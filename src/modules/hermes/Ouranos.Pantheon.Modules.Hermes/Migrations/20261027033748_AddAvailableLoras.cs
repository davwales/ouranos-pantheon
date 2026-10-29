using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Hermes.Migrations;

/// <inheritdoc />
public partial class AddAvailableLoras : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "trigger_phrase",
            schema: "hermes",
            table: "loras",
            type: "text",
            nullable: true
        );

        migrationBuilder.CreateTable(
            name: "available_loras",
            schema: "hermes",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                identifier = table.Column<string>(type: "text", nullable: false),
                title = table.Column<string>(type: "text", nullable: true),
                compat_class = table.Column<string>(type: "text", nullable: true),
                default_weight = table.Column<float>(type: "real", nullable: true),
                trigger_phrase = table.Column<string>(type: "text", nullable: true),
                usage_hint = table.Column<string>(type: "text", nullable: true),
                is_compatible = table.Column<bool>(type: "boolean", nullable: false),
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
                table.PrimaryKey("pk_available_loras", x => x.id);
            }
        );

        migrationBuilder.CreateIndex(
            name: "ix_available_loras_identifier",
            schema: "hermes",
            table: "available_loras",
            column: "identifier",
            unique: true
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "available_loras", schema: "hermes");

        migrationBuilder.DropColumn(name: "trigger_phrase", schema: "hermes", table: "loras");
    }
}

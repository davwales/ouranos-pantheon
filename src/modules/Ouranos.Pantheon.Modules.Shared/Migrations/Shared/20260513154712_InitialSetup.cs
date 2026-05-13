using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Shared.Migrations.Shared;

/// <inheritdoc />
public partial class InitialSetup : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: "shared");

        migrationBuilder.CreateTable(
            name: "notifications",
            schema: "shared",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                channel = table.Column<string>(type: "text", nullable: false),
                recipient = table.Column<string>(type: "text", nullable: false),
                subject = table.Column<string>(type: "text", nullable: false),
                message = table.Column<string>(type: "text", nullable: false),
                status = table.Column<int>(type: "integer", nullable: false),
                retry_count = table.Column<int>(type: "integer", nullable: false),
                sent_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: true
                ),
                last_error = table.Column<string>(type: "text", nullable: true),
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
                table.PrimaryKey("pk_notifications", x => x.id);
            }
        );

        migrationBuilder.CreateIndex(
            name: "ix_notifications_channel_status",
            schema: "shared",
            table: "notifications",
            columns: new[] { "channel", "status" }
        );

        migrationBuilder.CreateIndex(
            name: "ix_notifications_status",
            schema: "shared",
            table: "notifications",
            column: "status"
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "notifications", schema: "shared");
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Database.Migrations;

/// <inheritdoc />
public partial class RemoveTradeMessages : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "trade_message", schema: "plutus");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "trade_message",
            schema: "plutus",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                created_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false
                ),
                message_id = table.Column<Guid>(type: "uuid", nullable: false),
                trade_id = table.Column<Guid>(type: "uuid", nullable: false),
                updated_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false
                ),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_trade_message", x => x.id);
            }
        );

        migrationBuilder.CreateIndex(
            name: "ix_trade_message_message_id",
            schema: "plutus",
            table: "trade_message",
            column: "message_id",
            unique: true
        );
    }
}

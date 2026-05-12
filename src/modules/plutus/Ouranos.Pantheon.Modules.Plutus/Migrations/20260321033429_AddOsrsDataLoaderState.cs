using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Migrations;

/// <inheritdoc />
public partial class AddOsrsDataLoaderState : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "osrs_data_loader_states",
            schema: "plutus",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                last_processed = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: true
                ),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_osrs_data_loader_states", x => x.id);
            }
        );

        migrationBuilder.InsertData(
            schema: "plutus",
            table: "osrs_data_loader_states",
            columns: new[] { "id", "last_processed" },
            values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), null }
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "osrs_data_loader_states", schema: "plutus");
    }
}

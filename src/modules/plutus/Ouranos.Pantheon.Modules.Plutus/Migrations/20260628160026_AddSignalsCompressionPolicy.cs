using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Migrations;

/// <inheritdoc />
public partial class AddSignalsCompressionPolicy : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            SELECT add_compression_policy('plutus.signals',
                INTERVAL '7 days',
                if_not_exists => TRUE);
            """,
            suppressTransaction: true
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            SELECT remove_compression_policy('plutus.signals', if_exists => TRUE);
            """,
            suppressTransaction: true
        );
    }
}

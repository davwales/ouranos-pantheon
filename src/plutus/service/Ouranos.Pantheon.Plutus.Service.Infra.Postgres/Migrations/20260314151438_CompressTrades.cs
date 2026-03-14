using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Plutus.Service.Infra.Postgres.Migrations;

/// <inheritdoc />
public partial class CompressTrades : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE plutus.trades SET (
                timescaledb.compress,
                timescaledb.compress_segmentby = 'symbol_id'
            );
            """
        );

        migrationBuilder.Sql(
            """
            SELECT add_compression_policy('plutus.trades', INTERVAL '7 days');
            """
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            SELECT remove_compression_policy('plutus.trades');
            """
        );

        migrationBuilder.Sql(
            """
            ALTER TABLE plutus.trades SET (timescaledb.compress = false);
            """
        );
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Extensions;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Database.ModelConfigurations;

public sealed class TradeConfiguration : IEntityTypeConfiguration<Trade>
{
    public void Configure(EntityTypeBuilder<Trade> builder)
    {
        /*
        Trades are stored in a hypertable that requires the partional key to be included in the primary key.
        The following lines have been added to the initial migration to convert it to a hypertable.

        migrationBuilder.Sql(
            "SELECT create_hypertable('\"plutus\".\"trades\"', 'timestamp', migrate_data => TRUE);\n" +
            "CREATE INDEX ix_symbol_id_timestamp ON \"plutus\".\"trades\" (\"symbol_id\", \"timestamp\" DESC);"
        );
        */

        builder.HasKey(t => new { t.Id, t.Timestamp });

        builder.Property(t => t.Id).HasIdConversion();
        builder.Property(t => t.SymbolId).HasIdConversion();
    }
}
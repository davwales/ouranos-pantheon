using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Database.ModelConfigurations;

public sealed class LatestSignalConfiguration : IEntityTypeConfiguration<LatestSignal>
{
    public void Configure(EntityTypeBuilder<LatestSignal> builder)
    {
        builder.HasKey(e => new { e.SymbolId, e.SignalType });
        builder.ToView("latest_signals");
        builder.Property(e => e.SymbolId).HasIdConversion();
        builder.Property(e => e.SignalType).HasConversion<int>();
    }
}

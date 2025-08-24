namespace Ouranos.Pantheon.Plutus.DataLoader.Migration.Models;

public sealed record MigrationState(
    string Id
)
{
    public DateTimeOffset? LastMigratedTradeCreatedAt { get; set; }
}

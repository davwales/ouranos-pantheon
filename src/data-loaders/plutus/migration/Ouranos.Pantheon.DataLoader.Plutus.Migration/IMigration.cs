namespace Ouranos.Pantheon.DataLoader.Plutus.Migration;

public interface IMigration
{
    Task Migrate(CancellationToken cancellationToken);
}
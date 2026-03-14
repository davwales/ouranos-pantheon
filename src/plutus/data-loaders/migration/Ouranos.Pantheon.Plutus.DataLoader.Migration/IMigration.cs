namespace Ouranos.Pantheon.Plutus.DataLoader.Migration;

public interface IMigration
{
    Task Migrate(CancellationToken cancellationToken);
}
namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.DataLoaders;

public class OsrsDataLoaderState
{
    public static readonly Guid SingletonId = new("00000000-0000-0000-0000-000000000001");

    public Guid Id { get; init; }
    public DateTimeOffset? LastProcessed { get; set; }
}

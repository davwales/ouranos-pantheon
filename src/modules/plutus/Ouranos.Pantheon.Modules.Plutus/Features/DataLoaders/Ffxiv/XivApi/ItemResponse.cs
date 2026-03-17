namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.XivApi;

public sealed record ItemResponse
{
    public int Key { get; init; }

    public string Name { get; init; } = string.Empty;

    public bool CanBeHq { get; init; }
}

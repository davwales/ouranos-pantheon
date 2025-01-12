namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Infra.XivApi.Models;

public sealed record ItemResponse
{
    public int Key { get; init; }

    public string Name { get; init; } = string.Empty;

    public bool CanBeHq { get; init; }
}
namespace Ouranos.Pantheon.Plutus.DataLoader.Ffxiv.Infra.XivApi.Models;

public sealed record ItemResponse
{
    public int Key { get; init; }

    public string Name { get; init; } = string.Empty;

    public bool CanBeHq { get; init; }
}
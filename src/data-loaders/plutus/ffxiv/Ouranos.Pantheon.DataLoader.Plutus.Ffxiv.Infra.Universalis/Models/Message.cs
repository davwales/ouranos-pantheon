namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Infra.Universalis.Models;

public sealed record Message(
    string Event,
    int Item,
    int World,
    IReadOnlyCollection<ItemSale> Sales
);
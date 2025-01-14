using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Worker.Models;

namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Worker.Messages;

public sealed record SaleMessage(
    string Event,
    int Item,
    int World,
    IReadOnlyCollection<SaleDetail> Sales
);
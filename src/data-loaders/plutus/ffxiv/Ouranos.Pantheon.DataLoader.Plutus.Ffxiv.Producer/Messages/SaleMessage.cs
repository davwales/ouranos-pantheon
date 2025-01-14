using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Producer.Models;

namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Producer.Messages;

public sealed record SaleMessage(
    string Event,
    int Item,
    int World,
    IReadOnlyCollection<SaleDetail> Sales
);
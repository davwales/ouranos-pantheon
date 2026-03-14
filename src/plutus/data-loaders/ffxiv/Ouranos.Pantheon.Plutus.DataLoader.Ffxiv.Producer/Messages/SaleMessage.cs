using Ouranos.Pantheon.Plutus.DataLoader.Ffxiv.Producer.Models;

namespace Ouranos.Pantheon.Plutus.DataLoader.Ffxiv.Producer.Messages;

public sealed record SaleMessage(
    string Event,
    int Item,
    int World,
    IReadOnlyCollection<SaleDetail> Sales
);
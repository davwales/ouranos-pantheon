using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.Models;

namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.Messages;

public sealed record SaleMessage(
    string Event,
    int Item,
    int World,
    IReadOnlyCollection<SaleDetail> Sales
);

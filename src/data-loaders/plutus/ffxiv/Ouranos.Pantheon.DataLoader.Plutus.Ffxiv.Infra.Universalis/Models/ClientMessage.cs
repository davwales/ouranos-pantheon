namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Infra.Universalis.Models;

public sealed record ClientMessage(
    string Event,
    string Channel
);
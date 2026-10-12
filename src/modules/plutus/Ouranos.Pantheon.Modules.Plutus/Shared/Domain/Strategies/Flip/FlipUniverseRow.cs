namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;

internal sealed record FlipUniverseRow(Guid Id, string Name, string? Subcode, decimal BuyLimit);

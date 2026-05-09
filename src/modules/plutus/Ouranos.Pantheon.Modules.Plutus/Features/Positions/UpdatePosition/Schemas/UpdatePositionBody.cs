namespace Ouranos.Pantheon.Modules.Plutus.Features.Positions.UpdatePosition.Schemas;

public sealed record UpdatePositionBody(
    decimal Cost,
    decimal Quantity,
    string? Notes
);
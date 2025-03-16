namespace Ouranos.Pantheon.Service.Plutus.Domain.Markets;

public sealed record FlatTax(
    decimal Minimum,
    decimal Maximum,
    decimal Rate
);
namespace Ouranos.Pantheon.Plutus.Service.Domain.Markets;

public sealed record FlatTax(
    decimal Minimum,
    decimal Maximum,
    decimal Rate
);
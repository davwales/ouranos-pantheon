namespace Ouranos.Pantheon.Plutus.Service.Domain.Markets;

public record FlatTax(
    decimal Minimum,
    decimal Maximum,
    decimal Rate
);
namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;

public record FlatTax(
    decimal Minimum,
    decimal Maximum,
    decimal Rate
);

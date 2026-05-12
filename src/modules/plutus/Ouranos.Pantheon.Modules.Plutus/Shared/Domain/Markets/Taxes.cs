namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;

public record Taxes(FlatTax? Flat)
{
    protected Taxes()
        : this(Flat: null) { }
}

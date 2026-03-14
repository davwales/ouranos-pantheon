namespace Ouranos.Pantheon.Plutus.Service.Domain.Markets;

public record Taxes(FlatTax? Flat)
{
    protected Taxes() : this(Flat: null)
    {
    }
}
namespace Ouranos.Pantheon.Plutus.Service.Domain.Markets;

public sealed record Taxes(FlatTax? Flat)
{
    private Taxes() : this(Flat: null)
    {
    }
}
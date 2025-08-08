namespace Ouranos.Pantheon.Service.Plutus.Domain.Markets;

public sealed record Taxes(FlatTax? Flat)
{
    private Taxes() : this(Flat: null)
    {
    }
}
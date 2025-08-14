namespace Ouranos.Pantheon.Plutus.DataLoader.Stocks.Producer;

public sealed record AlpacaOptions(
    string ApiKey,
    string ApiSecret,
    IReadOnlyCollection<string> Symbols
)
{
    public const string SectionName = "Ouranos:Alpaca";

    public AlpacaOptions() : this(string.Empty, string.Empty, [])
    {
    }
}
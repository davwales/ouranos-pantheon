namespace Ouranos.Pantheon.DataLoader.Plutus.Stocks.Producer.Messages;

public sealed record ErrorMessage(
    int Code,
    string Msg
)
{
    public const string TypeIndicator = "error";
}
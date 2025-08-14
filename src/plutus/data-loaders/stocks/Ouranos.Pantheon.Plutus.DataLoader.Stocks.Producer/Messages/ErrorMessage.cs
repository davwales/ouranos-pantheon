namespace Ouranos.Pantheon.Plutus.DataLoader.Stocks.Producer.Messages;

public sealed record ErrorMessage(
    int Code,
    string Msg
)
{
    public const string TypeIndicator = "error";
}
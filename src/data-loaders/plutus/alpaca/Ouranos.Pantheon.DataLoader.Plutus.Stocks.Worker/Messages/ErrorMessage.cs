namespace Ouranos.Pantheon.DataLoader.Plutus.Stocks.Worker.Messages;

public sealed record ErrorMessage(
    int Code,
    string Msg
)
{
    public const string TypeIndicator = "error";
}
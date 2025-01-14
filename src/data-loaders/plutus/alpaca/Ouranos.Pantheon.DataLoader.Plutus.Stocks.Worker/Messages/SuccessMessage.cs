namespace Ouranos.Pantheon.DataLoader.Plutus.Stocks.Worker.Messages;

public sealed record SuccessMessage(string Msg)
{
    public const string TypeIndicator = "success";
}
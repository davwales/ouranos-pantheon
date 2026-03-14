namespace Ouranos.Pantheon.Plutus.DataLoader.Stocks.Producer.Messages;

public sealed record SuccessMessage(string? Msg)
{
    public const string TypeIndicator = "success";
}
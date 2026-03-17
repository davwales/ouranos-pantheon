namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Stocks.Messages;

public sealed record SuccessMessage(string? Msg)
{
    public const string TypeIndicator = "success";
}

namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Stocks.Messages;

public sealed record ErrorMessage(int Code, string Msg)
{
    public const string TypeIndicator = "error";
}

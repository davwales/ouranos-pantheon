using System.Text.Json.Serialization;

namespace Ouranos.Pantheon.DataLoader.Plutus.Stocks.Producer.Messages;

public sealed record AlpacaTradeMessage(
    [property: JsonPropertyName("S")] string SymbolCode,
    [property: JsonPropertyName("i")] int TradeId,
    [property: JsonPropertyName("x")] string ExchangeCode,
    [property: JsonPropertyName("p")] decimal Price,
    [property: JsonPropertyName("s")] int Size,
    [property: JsonPropertyName("c")] IReadOnlyCollection<string> Conditions,
    [property: JsonPropertyName("t")] DateTimeOffset Timestamp,
    [property: JsonPropertyName("z")] string Tape,
    [property: JsonPropertyName("vw")] decimal? Vwap
)
{
    public const string TypeIndicator = "t";
}
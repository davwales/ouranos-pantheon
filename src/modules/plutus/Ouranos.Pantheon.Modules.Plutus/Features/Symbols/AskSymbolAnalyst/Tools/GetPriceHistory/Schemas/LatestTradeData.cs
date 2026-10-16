namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Tools.GetPriceHistory.Schemas;

public sealed record LatestTradeData(decimal Price, decimal Volume, DateTimeOffset Timestamp);

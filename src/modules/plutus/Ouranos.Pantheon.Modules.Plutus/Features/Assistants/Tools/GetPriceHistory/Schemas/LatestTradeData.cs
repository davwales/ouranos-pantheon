namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetPriceHistory.Schemas;

public sealed record LatestTradeData(decimal Price, decimal Volume, DateTimeOffset Timestamp);

namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetPriceHistory.Schemas;

public sealed record PriceBucketData(
    DateTimeOffset Start,
    decimal AveragePrice,
    decimal LowPrice,
    decimal HighPrice,
    decimal Volume
);

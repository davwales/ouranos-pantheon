namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;

public sealed record PriceBucket(
    DateTimeOffset BucketStart,
    decimal AveragePrice,
    decimal MinPrice,
    decimal MaxPrice,
    decimal Volume
);

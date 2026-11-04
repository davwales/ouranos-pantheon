using Ardalis.GuardClauses;
using Microsoft.Extensions.Options;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals.Computers;

public sealed class PriceVelocitySignalComputer : ISignalComputer
{
    private readonly IOptions<SignalOptions> _options;

    public PriceVelocitySignalComputer(IOptions<SignalOptions> options)
    {
        Guard.Against.Null(options);

        _options = options;
    }

    public SignalType Type => SignalType.PriceVelocity;
    public string Label => "Price Velocity";
    public string Description => "Rate of price change clamped and normalised to [-1, 1].";

    public IReadOnlyList<InvestmentIntent> Intents =>
        [
            InvestmentIntent.Buy,
            InvestmentIntent.Sell,
            InvestmentIntent.Flip,
            InvestmentIntent.Merch,
        ];

    public decimal? Compute(SignalComputeContext context)
    {
        var buckets = context.PriceBuckets;
        if (buckets.Count < 2)
        {
            return null;
        }

        var firstPrice = buckets[0].AveragePrice;
        if (firstPrice == 0 || _options.Value.PriceVelocityThreshold == 0)
        {
            return null;
        }

        var lastPrice = buckets[^1].AveragePrice;
        var velocity = (lastPrice - firstPrice) / firstPrice;
        var value = Math.Clamp(velocity / _options.Value.PriceVelocityThreshold, -1m, 1m);

        return value;
    }
}

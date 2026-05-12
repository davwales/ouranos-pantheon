using Ardalis.GuardClauses;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.Shared;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals.Computers;

public sealed class BollingerBandsSignalComputer : ISignalComputer
{
    private readonly IOptions<SignalOptions> _options;

    public BollingerBandsSignalComputer(IOptions<SignalOptions> options)
    {
        Guard.Against.Null(options);

        _options = options;
    }

    public SignalType Type => SignalType.BollingerBands;
    public string Label => "Bollinger Bands";
    public string Description =>
        "%B position mapped to bullish (below lower band) or bearish (above upper band).";
    public IReadOnlyList<InvestmentIntent> Intents =>
        [InvestmentIntent.Buy, InvestmentIntent.Sell, InvestmentIntent.Merch];

    public Task<decimal?> ComputeAsync(SignalComputeContext context, CancellationToken ct)
    {
        var buckets = context.PriceBuckets;
        if (buckets.Count < 2)
        {
            return Task.FromResult<decimal?>(null);
        }

        var prices = buckets.Select(b => b.AveragePrice).ToArray();
        var mean = prices.Average();
        var variance = prices.Average(p => (p - mean) * (p - mean));
        var stdDev = (decimal)Math.Sqrt((double)variance);

        if (stdDev == 0)
        {
            return Task.FromResult<decimal?>(0m);
        }

        var multiplier = (decimal)_options.Value.BollingerMultiplier;
        var upper = mean + multiplier * stdDev;
        var lower = mean - multiplier * stdDev;

        if (upper == lower)
        {
            return Task.FromResult<decimal?>(null);
        }

        var currentPrice = buckets[^1].AveragePrice;
        var percentB = (currentPrice - lower) / (upper - lower);

        // Below lower band (%B < 0) = +1 (bullish), above upper band (%B > 1) = -1 (bearish)
        var value = Math.Clamp(1m - 2m * percentB, -1m, 1m);

        return Task.FromResult<decimal?>(value);
    }
}

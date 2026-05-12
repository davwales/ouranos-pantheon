using Ardalis.GuardClauses;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.Shared;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals.Computers;

public sealed class MovingAverageCrossoverSignalComputer : ISignalComputer
{
    private const int ShortPeriod = 5;
    private const int LongPeriod = 20;

    private readonly IOptions<SignalOptions> _options;

    public MovingAverageCrossoverSignalComputer(IOptions<SignalOptions> options)
    {
        Guard.Against.Null(options);

        _options = options;
    }

    public SignalType Type => SignalType.MovingAverageCrossover;
    public string Label => "Moving Average Crossover";
    public string Description => "SMA5 vs SMA20 divergence normalised to [-1, 1].";
    public IReadOnlyList<InvestmentIntent> Intents =>
        [InvestmentIntent.Sell, InvestmentIntent.Merch];

    public Task<decimal?> ComputeAsync(SignalComputeContext context, CancellationToken ct)
    {
        var buckets = context.PriceBuckets;
        if (buckets.Count < LongPeriod || _options.Value.MaCrossoverThreshold == 0)
        {
            return Task.FromResult<decimal?>(null);
        }

        var prices = buckets.Select(b => b.AveragePrice).TakeLast(LongPeriod).ToArray();
        var sma20 = prices.Average();

        if (sma20 == 0)
        {
            return Task.FromResult<decimal?>(null);
        }

        var sma5 = prices.TakeLast(ShortPeriod).Average();
        var divergence = (sma5 - sma20) / sma20;
        var value = Math.Clamp(divergence / _options.Value.MaCrossoverThreshold, -1m, 1m);

        return Task.FromResult<decimal?>(value);
    }
}

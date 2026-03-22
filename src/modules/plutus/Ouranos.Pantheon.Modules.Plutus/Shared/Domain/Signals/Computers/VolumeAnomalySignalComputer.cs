using Ardalis.GuardClauses;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.Shared;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals.Computers;

public sealed class VolumeAnomalySignalComputer : ISignalComputer
{
    private readonly IOptions<SignalOptions> _options;

    public VolumeAnomalySignalComputer(IOptions<SignalOptions> options)
    {
        Guard.Against.Null(options);

        _options = options;
    }

    public SignalType Type => SignalType.VolumeAnomaly;
    public string Label => "Volume Anomaly";
    public string Description => "Short-period volume rate vs long-period rate; spike indicates bullish activity.";

    public IReadOnlyList<InvestmentIntent> Intents =>
    [
        InvestmentIntent.Buy, InvestmentIntent.Sell, InvestmentIntent.Flip, InvestmentIntent.Merch
    ];

    public Task<decimal?> ComputeAsync(SignalComputeContext context, CancellationToken ct)
    {
        var shortSnap = context.ShortSnapshot;
        var longSnap = context.LongSnapshot;

        if (shortSnap is null || longSnap is null)
        {
            return Task.FromResult<decimal?>(null);
        }

        var shortWindowMinutes = (decimal)(_options.Value.ShortTimeFrame.ToTimeSpan()?.TotalMinutes ?? 0);
        var longWindowMinutes = (decimal)(_options.Value.LongTimeFrame.ToTimeSpan()?.TotalMinutes ?? 0);
        if (shortWindowMinutes == 0 || longWindowMinutes == 0)
        {
            return Task.FromResult<decimal?>(null);
        }

        var longRate = longSnap.TotalVolume / longWindowMinutes;
        if (longRate == 0 || _options.Value.VolumeAnomalyThreshold <= 1)
        {
            return Task.FromResult<decimal?>(null);
        }

        var shortRate = shortSnap.TotalVolume / shortWindowMinutes;
        var ratio = shortRate / longRate;
        var value = Math.Clamp(
            (ratio - 1m) / (_options.Value.VolumeAnomalyThreshold - 1m),
            -1m,
            1m
        );

        return Task.FromResult<decimal?>(value);
    }
}

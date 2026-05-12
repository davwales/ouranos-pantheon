using Ardalis.GuardClauses;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.Shared;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals.Computers;

public sealed class TrendMomentumSignalComputer : ISignalComputer
{
    private readonly IOptions<SignalOptions> _options;

    public TrendMomentumSignalComputer(IOptions<SignalOptions> options)
    {
        Guard.Against.Null(options);

        _options = options;
    }

    public SignalType Type => SignalType.TrendMomentum;
    public string Label => "Trend Momentum";
    public string Description => "Short average price vs long average price delta as a percentage.";
    public IReadOnlyList<InvestmentIntent> Intents =>
        [InvestmentIntent.Sell, InvestmentIntent.Merch];

    public Task<decimal?> ComputeAsync(SignalComputeContext context, CancellationToken ct)
    {
        var shortSnap = context.ShortSnapshot;
        var longSnap = context.LongSnapshot;

        if (
            shortSnap is null
            || longSnap is null
            || shortSnap.TotalVolume == 0
            || longSnap.TotalVolume == 0
        )
        {
            return Task.FromResult<decimal?>(null);
        }

        var longAvg = longSnap.TotalSpent / longSnap.TotalVolume;
        if (longAvg == 0 || _options.Value.MomentumThreshold == 0)
        {
            return Task.FromResult<decimal?>(null);
        }

        var shortAvg = shortSnap.TotalSpent / shortSnap.TotalVolume;
        var delta = (shortAvg - longAvg) / longAvg;
        var value = Math.Clamp(delta / _options.Value.MomentumThreshold, -1m, 1m);

        return Task.FromResult<decimal?>(value);
    }
}

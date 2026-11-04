using Ardalis.GuardClauses;
using Microsoft.Extensions.Options;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals.Computers;

public sealed class TaxAdjustedRoiSignalComputer : ISignalComputer
{
    private readonly IOptions<SignalOptions> _options;

    public TaxAdjustedRoiSignalComputer(IOptions<SignalOptions> options)
    {
        Guard.Against.Null(options);

        _options = options;
    }

    public SignalType Type => SignalType.TaxAdjustedRoi;
    public string Label => "Tax-Adjusted ROI";
    public string Description => "Return on investment after tax, normalised to [-1, 1].";
    public IReadOnlyList<InvestmentIntent> Intents => [InvestmentIntent.Flip, InvestmentIntent.Buy];

    public decimal? Compute(SignalComputeContext context)
    {
        var snapshot = context.MediumSnapshot;
        if (snapshot is null || snapshot.MinPrice == 0 || _options.Value.RoiThreshold == 0)
        {
            return null;
        }

        var tax = context.Rules.UnitTax(snapshot.MaxPrice);
        var roi = (snapshot.MaxPrice - snapshot.MinPrice - tax) / snapshot.MinPrice;
        var value = Math.Clamp(roi / _options.Value.RoiThreshold, -1m, 1m);

        return value;
    }
}

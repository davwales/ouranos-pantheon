using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;

/// <summary>
///     Side-aware 5-minute bars for one symbol, plus hourly running totals so quotes over any
///     whole-hour window are O(1).
/// </summary>
public sealed class FlipSeries
{
    private readonly double[] _highValue;
    private readonly double[] _highVolume;
    private readonly double[] _lowValue;
    private readonly double[] _lowVolume;
    private readonly int[] _pairedBars;

    public FlipSeries(
        Id<Symbol> symbolId,
        string name,
        string? subcode,
        decimal buyLimit,
        FlipBar[] bars
    )
    {
        Guard.Against.Null(bars);

        SymbolId = symbolId;
        Name = name;
        Subcode = subcode;
        BuyLimit = buyLimit;
        Bars = bars;

        var hours = bars.Length / FlipMarketData.BarsPerHour;
        _highValue = new double[hours + 1];
        _highVolume = new double[hours + 1];
        _lowValue = new double[hours + 1];
        _lowVolume = new double[hours + 1];
        _pairedBars = new int[hours + 1];

        for (var hour = 0; hour < hours; hour++)
        {
            double highValue = 0;
            double highVolume = 0;
            double lowValue = 0;
            double lowVolume = 0;

            var paired = 0;
            for (
                var i = hour * FlipMarketData.BarsPerHour;
                i < (hour + 1) * FlipMarketData.BarsPerHour;
                i++
            )
            {
                var bar = bars[i];
                highValue += (double)bar.HighPrice * bar.HighVolume;
                highVolume += bar.HighVolume;
                lowValue += (double)bar.LowPrice * bar.LowVolume;
                lowVolume += bar.LowVolume;

                if (bar.HighVolume > 0 && bar.LowVolume > 0)
                {
                    paired++;
                }
            }

            _highValue[hour + 1] = _highValue[hour] + highValue;
            _highVolume[hour + 1] = _highVolume[hour] + highVolume;
            _lowValue[hour + 1] = _lowValue[hour] + lowValue;
            _lowVolume[hour + 1] = _lowVolume[hour] + lowVolume;
            _pairedBars[hour + 1] = _pairedBars[hour] + paired;
        }
    }

    public Id<Symbol> SymbolId { get; }

    public string Name { get; }

    public string? Subcode { get; }

    public decimal BuyLimit { get; }

    public FlipBar[] Bars { get; }

    public int Hours => _pairedBars.Length - 1;

    /// <summary>Volume-weighted side prices and totals over hours [<paramref name="fromHour" />, <paramref name="toHour" />).</summary>
    public FlipWindow Window(int fromHour, int toHour)
    {
        fromHour = Math.Clamp(fromHour, 0, Hours);
        toHour = Math.Clamp(toHour, fromHour, Hours);

        var highVolume = _highVolume[toHour] - _highVolume[fromHour];
        var lowVolume = _lowVolume[toHour] - _lowVolume[fromHour];

        return new FlipWindow(
            highVolume > 0 ? (_highValue[toHour] - _highValue[fromHour]) / highVolume : 0,
            highVolume,
            lowVolume > 0 ? (_lowValue[toHour] - _lowValue[fromHour]) / lowVolume : 0,
            lowVolume,
            _pairedBars[toHour] - _pairedBars[fromHour]
        );
    }
}

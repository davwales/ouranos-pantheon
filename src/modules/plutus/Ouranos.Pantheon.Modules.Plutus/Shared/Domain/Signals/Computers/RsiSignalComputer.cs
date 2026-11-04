namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals.Computers;

public sealed class RsiSignalComputer : ISignalComputer
{
    private const int Period = 14;

    public SignalType Type => SignalType.Rsi;
    public string Label => "RSI";
    public string Description =>
        "RSI below 30 indicates oversold (bullish), above 70 indicates overbought (bearish).";
    public IReadOnlyList<InvestmentIntent> Intents => [InvestmentIntent.Buy, InvestmentIntent.Sell];

    public decimal? Compute(SignalComputeContext context)
    {
        var buckets = context.PriceBuckets;
        if (buckets.Count < Period + 1)
        {
            return null;
        }

        var prices = buckets.Select(b => b.AveragePrice).ToArray();

        var gains = new decimal[prices.Length - 1];
        var losses = new decimal[prices.Length - 1];

        for (var i = 1; i < prices.Length; i++)
        {
            var change = prices[i] - prices[i - 1];
            gains[i - 1] = change > 0 ? change : 0m;
            losses[i - 1] = change < 0 ? -change : 0m;
        }

        var avgGain = gains.Take(Period).Average();
        var avgLoss = losses.Take(Period).Average();

        for (var i = Period; i < gains.Length; i++)
        {
            avgGain = (avgGain * (Period - 1) + gains[i]) / Period;
            avgLoss = (avgLoss * (Period - 1) + losses[i]) / Period;
        }

        decimal rsi;
        if (avgGain == 0 && avgLoss == 0)
        {
            // A price that never moved is neither overbought nor oversold.
            rsi = 50m;
        }
        else if (avgLoss == 0)
        {
            rsi = 100m;
        }
        else
        {
            var rs = avgGain / avgLoss;
            rsi = 100m - (100m / (1m + rs));
        }

        // RSI < 30 = +1 (oversold/bullish), RSI > 70 = -1 (overbought/bearish)
        var value = Math.Clamp((50m - rsi) / 20m, -1m, 1m);

        return value;
    }
}

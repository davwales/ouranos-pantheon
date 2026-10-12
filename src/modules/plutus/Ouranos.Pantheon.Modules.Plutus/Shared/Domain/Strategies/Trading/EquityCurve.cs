namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Trading;

public sealed class EquityCurve(decimal startingEquity)
{
    private readonly List<decimal> _values = [];

    public IReadOnlyList<decimal> Values => _values;

    public decimal Peak { get; private set; } = startingEquity;

    /// <summary>Largest peak-to-trough fall so far, as a fraction of the peak.</summary>
    public decimal MaxDrawdown { get; private set; }

    public void Record(decimal equity)
    {
        _values.Add(equity);
        Peak = Math.Max(Peak, equity);
        if (Peak > 0)
        {
            MaxDrawdown = Math.Max(MaxDrawdown, (Peak - equity) / Peak);
        }
    }
}

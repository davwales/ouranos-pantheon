using Ardalis.GuardClauses;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Service.Plutus.Domain.Markets;

public sealed class Market : BaseEntity<Id<Market>>
{
    public Market(
        Id<Market> id,
        string name,
        Taxes taxes,
        bool isForecastingEnabled = false
    ) : base(id)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(taxes);

        Name = name;
        Taxes = taxes;
        IsForecastingEnabled = isForecastingEnabled;
    }

    public string Name { get; private set; }

    public Taxes Taxes { get; private set; }

    public bool IsForecastingEnabled { get; private set; }

    public void Update(string name, Taxes taxes)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(taxes);

        Name = name;
        Taxes = taxes;
    }
}
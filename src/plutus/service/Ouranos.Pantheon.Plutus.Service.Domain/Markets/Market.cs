using Ardalis.GuardClauses;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Plutus.Service.Domain.Markets;

public sealed class Market : BaseEntity<Id<Market>>
{
    private Market()
    {
    }

    public Market(
        Id<Market> id,
        string name,
        Taxes taxes,
        bool isForecastingEnabled = false,
        string? description = null,
        string? icon = null
    ) : base(id)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(taxes);

        Name = name;
        Taxes = taxes;
        IsForecastingEnabled = isForecastingEnabled;
        Description = description;
        Icon = icon;
    }

    public string Name { get; private set; }

    public Taxes Taxes { get; private set; }

    public bool IsForecastingEnabled { get; private set; }

    public string? Description { get; private set; }

    public string? Icon { get; private set; }

    public void Update(string name, Taxes taxes)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(taxes);

        Name = name;
        Taxes = taxes;
    }
}
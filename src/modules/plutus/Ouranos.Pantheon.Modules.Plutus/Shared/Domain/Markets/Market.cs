using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;

public sealed class Market : BaseEntity<Id<Market>>
{
    private Market(Id<Market> id)
        : base(id)
    {
        Name = string.Empty;
        Taxes = new Taxes(null);
    }

    public string Name { get; private set; }

    public Taxes Taxes { get; private set; }

    public bool IsForecastingEnabled { get; private set; }

    public string? Description { get; private set; }

    public string? Icon { get; private set; }

    public static Market Create(
        Id<Market> id,
        string name,
        Taxes taxes,
        bool isForecastingEnabled = false,
        string? description = null,
        string? icon = null
    )
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(taxes);

        return new Market(id)
        {
            Name = name,
            Taxes = taxes,
            IsForecastingEnabled = isForecastingEnabled,
            Description = description,
            Icon = icon,
        };
    }

    public void Update(string name, Taxes taxes)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(taxes);

        Name = name;
        Taxes = taxes;
        Update();
    }
}

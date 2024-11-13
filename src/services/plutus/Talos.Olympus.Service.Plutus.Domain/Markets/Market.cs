using Talos.Olympus.Core.Domain.Common;

namespace Talos.Olympus.Service.Plutus.Domain.Markets;

public sealed class Market : BaseEntity<Id<Market>>
{
    public Market(
        Id<Market> id,
        string name,
        Taxes taxes
    ) : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(taxes);

        Name = name;
        Taxes = taxes;
    }

    public string Name { get; private set; }

    public Taxes Taxes { get; private set; }

    public void Update(string name, Taxes taxes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(taxes);

        Name = name;
        Taxes = taxes;
    }
}
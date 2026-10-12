using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

public sealed class Strategy : BaseEntity<Id<Strategy>>
{
    private Strategy(Id<Strategy> id)
        : base(id)
    {
        Name = string.Empty;
        Configuration = new FlipConfiguration();
    }

    public Id<Market> MarketId { get; init; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public StrategyConfiguration Configuration { get; private set; }

    public bool IsActive { get; private set; } = true;

    private Market? _market;
    public Market Market => _market ?? throw new NavigationPropertyNotLoadedException<Strategy>();

    public static Strategy Create(
        Id<Market> marketId,
        string name,
        string? description,
        StrategyConfiguration configuration,
        Market? market = null
    )
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(configuration);
        configuration.Validate();

        if (market is not null)
        {
            Guard.Against.InvalidInput(market, nameof(market), m => m.Id == marketId);
        }

        return new Strategy(DatabaseExtensions.CreateId<Strategy>())
        {
            MarketId = marketId,
            Name = name,
            Description = description,
            Configuration = configuration,
            _market = market,
        };
    }

    public void Update(string name, string? description, StrategyConfiguration configuration)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(configuration);
        configuration.Validate();

        Name = name;
        Description = description;
        Configuration = configuration;

        Update();
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        Update();
    }
}

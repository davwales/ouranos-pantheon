using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Shared.Extensions;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

public class Strategy : BaseEntity<Id<Strategy>>
{
    protected Strategy(Id<Strategy> id) : base(id)
    {
        Name = string.Empty;
        Configuration = new StrategyConfiguration();
    }

    public Id<Market> MarketId { get; init; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public StrategyType Type { get; init; }

    public StrategyConfiguration Configuration { get; private set; }

    public bool IsActive { get; private set; } = true;

    private Market? _market;
    public Market Market => _market ?? throw new NavigationPropertyNotLoadedException<Strategy>();

    public static Strategy Create(
        Id<Market> marketId,
        string name,
        string? description,
        StrategyType type,
        StrategyConfiguration configuration,
        Market? market = null
    )
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(configuration);

        if (market is not null)
        {
            Guard.Against.InvalidInput(market, nameof(market), m => m.Id == marketId);
        }

        return new Strategy(DatabaseExtensions.CreateId<Strategy>())
        {
            MarketId = marketId,
            Name = name,
            Description = description,
            Type = type,
            Configuration = configuration,
            _market = market
        };
    }

    public void Update(string name, string? description, StrategyConfiguration configuration)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(configuration);

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

using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Shared.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

public sealed class Strategy : BaseEntity<Id<Strategy>>
{
    private Strategy(Id<Strategy> id)
        : base(id)
    {
        Name = string.Empty;
        TradingConfiguration = new TradingConfiguration();
    }

    public Id<Market> MarketId { get; init; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public StrategyType Type { get; init; }

    public TradingConfiguration TradingConfiguration { get; set; }

    public SignalWeightedConfig? SignalWeightedConfig { get; set; }
    public ForecastMomentumConfig? ForecastMomentumConfig { get; set; }
    public MeanReversionConfig? MeanReversionConfig { get; set; }
    public RecipeArbitrageConfig? RecipeArbitrageConfig { get; set; }

    private List<CompositeComponent>? _components;
    public List<CompositeComponent> Components => _components ??= [];

    public bool IsActive { get; private set; } = true;

    private Market? _market;
    public Market Market => _market ?? throw new NavigationPropertyNotLoadedException<Strategy>();

    public static Strategy Create(
        Id<Market> marketId,
        string name,
        string? description,
        StrategyType type,
        TradingConfiguration tradingConfiguration,
        SignalWeightedConfig? signalWeightedConfig = null,
        ForecastMomentumConfig? forecastMomentumConfig = null,
        MeanReversionConfig? meanReversionConfig = null,
        RecipeArbitrageConfig? recipeArbitrageConfig = null,
        Market? market = null,
        List<CompositeComponent>? components = null
    )
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(tradingConfiguration);

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
            TradingConfiguration = tradingConfiguration,
            SignalWeightedConfig = signalWeightedConfig,
            ForecastMomentumConfig = forecastMomentumConfig,
            MeanReversionConfig = meanReversionConfig,
            RecipeArbitrageConfig = recipeArbitrageConfig,
            _market = market,
            _components = components,
        };
    }

    public void Update(
        string name,
        string? description,
        TradingConfiguration tradingConfiguration,
        SignalWeightedConfig? signalWeightedConfig = null,
        ForecastMomentumConfig? forecastMomentumConfig = null,
        MeanReversionConfig? meanReversionConfig = null,
        RecipeArbitrageConfig? recipeArbitrageConfig = null
    )
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(tradingConfiguration);

        Name = name;
        Description = description;
        TradingConfiguration = tradingConfiguration;
        SignalWeightedConfig = signalWeightedConfig;
        ForecastMomentumConfig = forecastMomentumConfig;
        MeanReversionConfig = meanReversionConfig;
        RecipeArbitrageConfig = recipeArbitrageConfig;

        Update();
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        Update();
    }
}

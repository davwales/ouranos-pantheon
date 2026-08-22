using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

public sealed class Strategy : BaseEntity<Id<Strategy>>
{
    private Strategy(Id<Strategy> id)
        : base(id)
    {
        Name = string.Empty;
        TradingConfiguration = new TradingConfiguration();
        InputWeights = [];
        Thresholds = new InputThresholds();
    }

    public Id<Market> MarketId { get; init; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    /// <summary>
    ///     Weighted vector of every input this strategy blends. At least one entry
    ///     with a non-zero weight is required for the strategy to produce scores.
    ///     Weights are relative; the executor normalizes by total weight at score time.
    /// </summary>
    public List<InputWeight> InputWeights { get; private set; } = [];

    public InputThresholds Thresholds { get; private set; } = new();

    public TradingConfiguration TradingConfiguration { get; private set; }

    public bool IsActive { get; private set; } = true;

    private Market? _market;
    public Market Market => _market ?? throw new NavigationPropertyNotLoadedException<Strategy>();

    public static Strategy Create(
        Id<Market> marketId,
        string name,
        string? description,
        TradingConfiguration tradingConfiguration,
        List<InputWeight>? inputWeights,
        InputThresholds? thresholds,
        Market? market = null
    )
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(tradingConfiguration);
        Guard.Against.Null(inputWeights);
        Guard.Against.InvalidInput(
            inputWeights,
            nameof(inputWeights),
            w => w.Any(i => i.Weight != 0m),
            "Strategy must have at least one input weight with a non-zero weight."
        );
        Guard.Against.InvalidInput(
            inputWeights,
            nameof(inputWeights),
            w => w.Select(i => i.Kind).Distinct().Count() == w.Count,
            "Strategy input weights must not contain duplicate input kinds."
        );

        if (market is not null)
        {
            Guard.Against.InvalidInput(market, nameof(market), m => m.Id == marketId);
        }

        return new Strategy(DatabaseExtensions.CreateId<Strategy>())
        {
            MarketId = marketId,
            Name = name,
            Description = description,
            TradingConfiguration = tradingConfiguration,
            InputWeights = inputWeights,
            Thresholds = thresholds ?? new InputThresholds(),
            _market = market,
        };
    }

    public void Update(
        string name,
        string? description,
        TradingConfiguration tradingConfiguration,
        List<InputWeight> inputWeights,
        InputThresholds? thresholds
    )
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(tradingConfiguration);
        Guard.Against.Null(inputWeights);
        Guard.Against.InvalidInput(
            inputWeights,
            nameof(inputWeights),
            w => w.Any(i => i.Weight != 0m),
            "Strategy must have at least one input weight with a non-zero weight."
        );
        Guard.Against.InvalidInput(
            inputWeights,
            nameof(inputWeights),
            w => w.Select(i => i.Kind).Distinct().Count() == w.Count,
            "Strategy input weights must not contain duplicate input kinds."
        );

        Name = name;
        Description = description;
        TradingConfiguration = tradingConfiguration;
        InputWeights = inputWeights;
        Thresholds = thresholds ?? new InputThresholds();

        Update();
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        Update();
    }
}

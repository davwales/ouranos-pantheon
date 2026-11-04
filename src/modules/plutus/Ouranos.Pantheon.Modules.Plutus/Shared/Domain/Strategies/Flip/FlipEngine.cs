using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Engine;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;

public sealed class FlipEngine(IFlipDataQueryService dataService)
    : StrategyEngine<FlipConfiguration, FlipMarketData>
{
    /// <summary>Extra history loaded so quotes still work when the newest data lags behind.</summary>
    public const int MaxDataDelayHours = 6;

    private static readonly Parameter[] Parameters =
    [
        new("lookbackHours", 4, 72, IsWhole: true),
        new("bidOffset", -0.3, 0.4),
        new("askOffset", -0.3, 0.4),
        new("minMargin", 0, 0.15),
        new("maxHoldHours", 12, 168, IsWhole: true),
        new("maxPositionPercent", 0.02, 0.5),
        new("maxPriceMove", 0, 0.3),
    ];

    private readonly IFlipDataQueryService _dataService = Guard.Against.Null(dataService);

    public override StrategyMode Mode => StrategyMode.Flip;

    public override int OptimizationSegmentDays => 7;

    /// <summary>
    ///     History needed before the first check-in. The 7-day price-move window is always loaded
    ///     because the optimizer may switch the check on.
    /// </summary>
    public static DateTimeOffset WarmUpStart(DateTimeOffset start, FlipConfiguration configuration)
    {
        return start.AddHours(
            -Math.Max(configuration.LookbackHours, FlipQuoter.PriceMoveDays * 24)
        );
    }

    /// <summary>
    ///     Quotes from the newest data rather than the clock, ignoring the hour still being
    ///     filled, so a lagging price feed shifts the quote window instead of emptying it.
    /// </summary>
    public static int LastCompleteHour(FlipMarketData data)
    {
        Guard.Against.Null(data);

        var last = -1;
        foreach (var series in data.Series)
        {
            for (var i = series.Bars.Length - 1; i > last; i--)
            {
                if (series.Bars[i].HighVolume > 0 || series.Bars[i].LowVolume > 0)
                {
                    last = i;
                    break;
                }
            }
        }

        return Math.Max(0, last / FlipMarketData.BarsPerHour);
    }

    protected override Task<FlipMarketData> LoadAsync(
        Id<Market> marketId,
        DateTimeOffset start,
        DateTimeOffset end,
        FlipConfiguration configuration,
        CancellationToken cancellationToken
    )
    {
        return _dataService.LoadAsync(
            marketId,
            WarmUpStart(start, configuration),
            end,
            cancellationToken
        );
    }

    protected override SimulationResult Simulate(
        FlipMarketData data,
        FlipConfiguration configuration,
        SimulationSettings settings
    )
    {
        return FlipSimulator.Run(data, configuration, settings);
    }

    /// <summary>
    ///     The trader's own constraints (check-in cadence, open order limit, minimum price) are kept from the
    ///     seed and never evolved.
    /// </summary>
    protected override ParameterSpace CreateParameterSpace(FlipConfiguration seed)
    {
        return new ParameterSpace(
            Parameters,
            [
                seed.LookbackHours,
                (double)seed.BidOffset,
                (double)seed.AskOffset,
                (double)seed.MinMargin,
                seed.MaxHoldHours,
                (double)seed.MaxPositionPercent,
                (double)seed.MaxPriceMove,
            ],
            genes =>
                seed with
                {
                    LookbackHours = (int)genes[0],
                    BidOffset = ParameterSpace.ToDecimal(genes[1]),
                    AskOffset = ParameterSpace.ToDecimal(genes[2]),
                    MinMargin = ParameterSpace.ToDecimal(genes[3]),
                    MaxHoldHours = (int)genes[4],
                    MaxPositionPercent = ParameterSpace.ToDecimal(genes[5]),
                    MaxPriceMove = ParameterSpace.ToDecimal(genes[6]),
                }
        );
    }

    /// <summary>One buy order per free order slot, ranked and sized as the simulator would at a check-in now.</summary>
    protected override async Task<IReadOnlyList<StrategyRecommendation>> RecommendAsync(
        Id<Market> marketId,
        FlipConfiguration configuration,
        decimal budget,
        decimal participationRate,
        CancellationToken cancellationToken
    )
    {
        var now = DateTimeOffset.UtcNow;

        var data = await _dataService.LoadAsync(
            marketId,
            WarmUpStart(now, configuration).AddHours(-MaxDataDelayHours),
            now,
            cancellationToken
        );

        var endHour = LastCompleteHour(data);

        var candidates = FlipQuoter.Rank(
            data.Series,
            endHour,
            configuration,
            data.Rules,
            participationRate
        );

        var positionBudget = budget * configuration.MaxPositionPercent;
        var remaining = budget;
        var recommendations = new List<StrategyRecommendation>();

        foreach (
            var (candidate, volume) in FlipSimulator.PlanOrders(
                candidates,
                configuration.MaxOpenOrders,
                positionBudget,
                budget,
                _ => 0m
            )
        )
        {
            var quote = candidate.Quote;

            var limitCap =
                quote.Series.BuyLimit * configuration.CheckInHours / FlipQuoter.BuyLimitHours;

            var limitedBy =
                Math.Min(positionBudget, remaining) / quote.Bid < candidate.ExpectedUnits
                    ? PositionLimit.Budget
                : candidate.ExpectedUnits >= limitCap ? PositionLimit.BuyLimit
                : PositionLimit.Liquidity;

            var window = quote.Series.Window(endHour - configuration.LookbackHours, endHour);
            var allocation = volume * quote.Bid;
            remaining -= allocation;

            recommendations.Add(
                new StrategyRecommendation(
                    quote.Series.SymbolId.ToString(),
                    quote.Series.Name,
                    quote.Series.Subcode,
                    quote.NetMargin,
                    allocation,
                    quote.LowPrice,
                    volume,
                    (decimal)window.LowVolume / configuration.LookbackHours * 24,
                    limitedBy,
                    $"Buy at {quote.Bid:N0}, sell at {quote.Ask:N0}: {quote.NetMargin:P1} after tax; "
                        + $"about {candidate.ExpectedUnits:N0} would have filled per {configuration.CheckInHours}h check-in.",
                    quote.Bid,
                    quote.Ask,
                    quote.NetMargin,
                    allocation * quote.NetMargin
                )
            );
        }

        return recommendations;
    }
}

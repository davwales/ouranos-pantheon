using Ardalis.GuardClauses;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Engine;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Swing;

public sealed class SwingEngine(
    ISwingDataQueryService dataService,
    SwingSignalReconstructor signalReconstructor,
    IOptions<BacktestDataOptions> options
) : StrategyEngine<SwingConfiguration, SwingData>
{
    private const int TopSignalCount = 3;

    internal static readonly Parameter MaxPositions = new("maxPositions", 8, 60, IsWhole: true);
    internal static readonly Parameter MaxPositionPercent = new("maxPositionPercent", 0.01, 0.15);
    internal static readonly Parameter HoldPeriodDays = new("holdPeriodDays", 1, 60, IsWhole: true);
    internal static readonly Parameter BuyThreshold = new("buyThreshold", 0, 0.95);
    internal static readonly Parameter SellThreshold = new("sellThreshold", -0.9, 0.3);

    private readonly ISwingDataQueryService _dataService = Guard.Against.Null(dataService);
    private readonly SwingSignalReconstructor _signalReconstructor = Guard.Against.Null(
        signalReconstructor
    );
    private readonly BacktestDataOptions _options = Guard.Against.Null(options).Value;

    public override StrategyMode Mode => StrategyMode.Swing;

    public override int OptimizationSegmentDays => 30;

    protected override async Task<SwingData> LoadAsync(
        Id<Market> marketId,
        DateTimeOffset start,
        DateTimeOffset end,
        SwingConfiguration configuration,
        CancellationToken cancellationToken
    )
    {
        var market = await _dataService.LoadDataAsync(
            marketId,
            start,
            end,
            _options.LookbackDays,
            cancellationToken
        );

        var signals = await _signalReconstructor.BuildAsync(
            market,
            DateOnly.FromDateTime(start.UtcDateTime),
            DateOnly.FromDateTime(end.UtcDateTime),
            cancellationToken
        );

        return new SwingData(market, signals);
    }

    protected override SimulationResult Simulate(
        SwingData data,
        SwingConfiguration configuration,
        SimulationSettings settings
    )
    {
        return SwingSimulator.Run(data, configuration, settings, _options);
    }

    /// <summary>
    ///     Only the inputs the strategy already uses are tuned, so optimizing a strategy built on a
    ///     chosen subset of signals tunes exactly those signals. Weights may turn negative to use an
    ///     input contrarily.
    /// </summary>
    protected override ParameterSpace CreateParameterSpace(SwingConfiguration seed)
    {
        var kinds = seed.InputWeights.Select(w => w.Kind).Distinct().Order().ToList();
        var weights = seed
            .InputWeights.GroupBy(w => w.Kind)
            .ToDictionary(g => g.Key, g => g.First().Weight);

        List<Parameter> parameters =
        [
            MaxPositions,
            MaxPositionPercent,
            HoldPeriodDays,
            .. kinds.Select(kind => new Parameter($"weight.{kind}", -3, 3)),
            BuyThreshold,
            SellThreshold,
        ];

        List<double> values =
        [
            seed.MaxPositions ?? MaxPositions.Max,
            (double)(seed.MaxPositionPercent ?? 1m),
            seed.HoldPeriodDays ?? HoldPeriodDays.Max,
            .. kinds.Select(kind => (double)weights[kind]),
            (double)(seed.BuyThreshold ?? 0m),
            (double)(seed.SellThreshold ?? 0m),
        ];

        return new ParameterSpace(
            parameters,
            values,
            genes => new SwingConfiguration(
                [
                    .. kinds.Select(
                        (kind, i) => new InputWeight(kind, ParameterSpace.ToDecimal(genes[3 + i]))
                    ),
                ],
                ParameterSpace.ToDecimal(genes[3 + kinds.Count]),
                ParameterSpace.ToDecimal(genes[4 + kinds.Count]),
                (int)genes[0],
                ParameterSpace.ToDecimal(genes[1]),
                (int)genes[2]
            )
        );
    }

    /// <summary>
    ///     Scores today's partial trading against the last days of signal history, then picks and
    ///     sizes symbols exactly as the simulator would on its next day with nothing held.
    /// </summary>
    protected override async Task<IReadOnlyList<StrategyRecommendation>> RecommendAsync(
        Id<Market> marketId,
        SwingConfiguration configuration,
        decimal budget,
        decimal participationRate,
        CancellationToken cancellationToken
    )
    {
        var now = DateTimeOffset.UtcNow;
        var decisionDay = DateOnly.FromDateTime(now.UtcDateTime).AddDays(1);
        var firstDay = decisionDay.AddDays(-SwingScoring.SignalHistoryWindowSize);

        var decisionDate = new DateTimeOffset(
            decisionDay.ToDateTime(TimeOnly.MinValue),
            TimeSpan.Zero
        );

        var market = await _dataService.LoadDataAsync(
            marketId,
            decisionDate.AddDays(-SwingScoring.SignalHistoryWindowSize),
            now,
            _options.LookbackDays,
            cancellationToken
        );

        var signals = await _signalReconstructor.BuildAsync(
            market,
            firstDay,
            decisionDay,
            cancellationToken
        );

        var inputs = SwingScoring.ScoreInputs(signals, firstDay, decisionDay)[decisionDay];

        var candidates = market
            .Symbols.Where(s => inputs.TryGetValue(s.Id, out var i) && i.IsEligible)
            .Select(s =>
                (
                    Symbol: s,
                    Score: SwingScoring.Combine(inputs[s.Id].Scores, configuration.InputWeights)
                )
            )
            .Where(s => s.Score.HasValue)
            .Select(s => new SwingCandidate(
                s.Symbol,
                s.Score.GetValueOrDefault(),
                inputs[s.Symbol.Id].Price
            ));

        var entries = SwingSimulator.PlanEntries(
            candidates,
            configuration,
            _ => false,
            0,
            budget,
            (candidate, positionBudget) =>
            {
                var size = PositionSizer.Size(
                    candidate.Price,
                    positionBudget,
                    Math.Floor(
                        market.GetTypicalDailyVolume(candidate.Symbol.Id, decisionDate)
                            * participationRate
                    ),
                    candidate.Symbol.AdditionalFields.Limit,
                    _options.LimitWindowsPerDay,
                    _options.MinFillFraction
                );
                return size.Volume < 1
                    ? null
                    : new SwingEntry(candidate, size.Volume, candidate.Price, size.LimitedBy);
            }
        );

        return
        [
            .. entries.Select(entry =>
            {
                var symbol = entry.Candidate.Symbol;
                return new StrategyRecommendation(
                    symbol.Id.ToString(),
                    symbol.Name,
                    symbol.Subcode,
                    entry.Candidate.Score,
                    entry.Cost,
                    entry.Price,
                    entry.Volume,
                    market.GetTypicalDailyVolume(symbol.Id, decisionDate),
                    entry.LimitedBy,
                    Rationale(entry.Candidate, signals.On(decisionDay)[symbol.Id], market, now)
                );
            }),
        ];
    }

    private static string Rationale(
        SwingCandidate candidate,
        SwingSignalDay signals,
        SwingMarketData market,
        DateTimeOffset now
    )
    {
        var parts = new List<string> { $"Score: {candidate.Score:F3}" };

        if (signals.Signals.Count > 0)
        {
            var top = signals
                .Signals.OrderByDescending(s => Math.Abs(s.Value))
                .Take(TopSignalCount)
                .Select(s => $"{s.Key}={s.Value:F2}");
            parts.Add($"Top signals: {string.Join(", ", top)}");
        }

        if (market.GetSnapshotsForSymbol(candidate.Symbol.Id, now).Medium is { } week)
        {
            parts.Add($"Price range: {week.MinPrice:F2}-{week.MaxPrice:F2}");
        }

        return string.Join("; ", parts);
    }
}

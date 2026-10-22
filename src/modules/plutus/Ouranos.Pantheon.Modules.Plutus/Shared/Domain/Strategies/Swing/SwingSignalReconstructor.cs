using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Swing;

/// <summary>
///     Re-runs the live signal computers against daily aggregates for every decision day. A
///     decision on day D only sees trades up to the end of D-1, so it never uses the prices or
///     volume its fills execute against.
/// </summary>
public sealed class SwingSignalReconstructor(IEnumerable<ISignalComputer> signalComputers)
{
    private const int SignalWindowDays = 30;

    private readonly IReadOnlyList<ISignalComputer> _signalComputers =
    [
        .. Guard.Against.Null(signalComputers),
    ];

    public async Task<SwingSignalTable> BuildAsync(
        SwingMarketData data,
        DateOnly first,
        DateOnly last,
        CancellationToken cancellationToken
    )
    {
        Guard.Against.Null(data);

        var days = new Dictionary<DateOnly, IReadOnlyDictionary<Id<Symbol>, SwingSignalDay>>();
        for (var day = first; day <= last; day = day.AddDays(1))
        {
            cancellationToken.ThrowIfCancellationRequested();
            days[day] = await BuildDayAsync(data, day, cancellationToken);
        }

        return new SwingSignalTable(days);
    }

    private async Task<Dictionary<Id<Symbol>, SwingSignalDay>> BuildDayAsync(
        SwingMarketData data,
        DateOnly day,
        CancellationToken cancellationToken
    )
    {
        var decisionDate = new DateTimeOffset(day.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var signalDate = decisionDate.AddDays(-1);
        var symbols = new Dictionary<Id<Symbol>, SwingSignalDay>();

        foreach (var symbol in data.Symbols)
        {
            var aggregates = data.GetWindowAggregates(
                symbol.Id,
                DateTimeOffset.MinValue,
                signalDate,
                windowDays: SignalWindowDays
            );

            if (aggregates.Count == 0)
            {
                continue;
            }

            var price = aggregates.MaxBy(a => a.Date)?.AveragePrice ?? 0m;
            if (price == 0m)
            {
                continue;
            }

            var (Short, Medium, Long) = data.GetSnapshotsForSymbol(symbol.Id, signalDate);

            var context = new SignalComputeContext(
                symbol.Id,
                Short?.MarketId ?? default,
                data.Rules,
                symbol.AdditionalFields.Limit ?? 0m,
                Short,
                Medium,
                Long,
                SwingMath.BuildPriceBucketsFromAggregates(aggregates)
            );

            var signals = new Dictionary<SignalType, decimal>();
            foreach (var computer in _signalComputers)
            {
                var value = await computer.ComputeAsync(context, cancellationToken);
                if (value.HasValue)
                {
                    signals[computer.Type] = value.Value;
                }
            }

            symbols[symbol.Id] = new SwingSignalDay(
                price,
                data.GetTypicalDailyVolume(symbol.Id, decisionDate) > 0,
                signals
            );
        }

        return symbols;
    }
}

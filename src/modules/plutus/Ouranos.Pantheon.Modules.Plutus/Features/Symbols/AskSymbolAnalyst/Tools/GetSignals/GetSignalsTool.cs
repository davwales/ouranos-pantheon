using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignalHistory.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Tools.GetSignals.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Tools.GetSignals;

public sealed class GetSignalsTool(ILogger<GetSignalsTool> logger, IMessageBus bus)
    : AssistantDataTool<SymbolAnalystContext, AssistantToolNoArguments, SignalsData>(logger)
{
    private readonly IMessageBus _bus = Guard.Against.Null(bus);

    public override string Name => "get_signals";

    public override string Description =>
        "Call this before explaining, weighing or recommending from this symbol's signals. Returns "
        + "the overall score and every signal (-1 bearish to 1 bullish) with its direction, "
        + "strength, intents and weekly trend (oldest to latest daily value).";

    protected override async ValueTask<AssistantData<SignalsData>> QueryAsync(
        SymbolAnalystContext context,
        AssistantToolNoArguments arguments,
        CancellationToken cancellationToken
    )
    {
        var signals = _bus.InvokeAsync<GetSymbolSignalsResponse>(
            new GetSymbolSignalsInput(context.SymbolId),
            cancellationToken
        );

        var history = _bus.InvokeAsync<GetSymbolSignalHistoryResponse>(
            new GetSymbolSignalHistoryInput(context.SymbolId),
            cancellationToken
        );

        var current = await signals;
        var trends = (await history)
            .Signals.GroupBy(signal => signal.Type)
            .ToDictionary(group => group.Key, group => WeeklyTrend(group.First()));

        return new SignalsData(
            current.Summary,
            [
                .. current.Signals.Select(signal =>
                    SymbolAnalystData.Signal(signal, trends.GetValueOrDefault(signal.Type))
                ),
            ]
        );
    }

    private static IReadOnlyList<decimal>? WeeklyTrend(SignalHistoryResponse signal)
    {
        var daily = signal
            .History.GroupBy(point => point.ComputedAt.UtcDateTime.Date)
            .OrderBy(day => day.Key)
            .Select(day => day.OrderBy(point => point.ComputedAt).Last().Value)
            .ToList();

        if (daily.Count < 2)
        {
            return null;
        }

        int[] picks = [0, daily.Count / 2, daily.Count - 1];
        return [.. picks.Distinct().Select(i => daily[i])];
    }
}

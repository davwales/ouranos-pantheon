using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Scopes;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetActivityHours.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetVolumeHeatmap.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetActivityHours;

public sealed class GetActivityHoursTool(ILogger<GetActivityHoursTool> logger, IMessageBus bus)
    : AssistantDataTool<IMarketScope, AssistantToolNoArguments, ActivityHoursData>(logger)
{
    internal const int LookbackWeeks = 4;

    internal const int Slots = 3;

    internal const string NoTrades = "No trades in the last few weeks.";

    private readonly IMessageBus _bus = Guard.Against.Null(bus);

    public override string Name => "get_activity_hours";

    public override string Description =>
        "The busiest and quietest trading hours of the week in this market (UTC), with each "
        + "hour's share of the trades.";

    protected override async ValueTask<AssistantData<ActivityHoursData>> QueryAsync(
        IMarketScope context,
        AssistantToolNoArguments arguments,
        CancellationToken cancellationToken
    )
    {
        var heatmap = await _bus.InvokeAsync<GetVolumeHeatmapResponse>(
            new GetVolumeHeatmapInput(context.MarketId, LookbackWeeks),
            cancellationToken
        );

        var cells = heatmap.Rows.Where(cell => cell.TotalTrades > 0).ToList();

        if (cells.Count == 0)
        {
            return AssistantData<ActivityHoursData>.WithNote(NoTrades);
        }

        return new ActivityHoursData(
            LookbackWeeks,
            [.. cells.OrderByDescending(cell => cell.TotalTrades).Take(Slots).Select(Slot)],
            [.. cells.OrderBy(cell => cell.TotalTrades).Take(Slots).Select(Slot)]
        );
    }

    // The heatmap numbers days from Monday (0) to Sunday (6).
    private static ActivitySlotData Slot(HeatmapCellResponse cell)
    {
        return new ActivitySlotData(
            (DayOfWeek)((cell.DayOfWeek + 1) % 7),
            cell.Hour,
            cell.Percentage
        );
    }
}

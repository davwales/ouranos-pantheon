namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Tools.GetActivityHours.Schemas;

public sealed record ActivityHoursData(
    int LookbackWeeks,
    IReadOnlyList<ActivitySlotData> Busiest,
    IReadOnlyList<ActivitySlotData> Quietest
);

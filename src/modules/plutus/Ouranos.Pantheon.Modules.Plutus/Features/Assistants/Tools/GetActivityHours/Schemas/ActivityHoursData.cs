namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetActivityHours.Schemas;

public sealed record ActivityHoursData(
    int LookbackWeeks,
    IReadOnlyList<ActivitySlotData> Busiest,
    IReadOnlyList<ActivitySlotData> Quietest
);

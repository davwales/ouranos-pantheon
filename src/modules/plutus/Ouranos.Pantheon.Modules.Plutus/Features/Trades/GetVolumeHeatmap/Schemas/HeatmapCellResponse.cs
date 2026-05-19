namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetVolumeHeatmap.Schemas;

public sealed record HeatmapCellResponse(
    int DayOfWeek,
    int Hour,
    long TotalTrades,
    decimal Percentage
);

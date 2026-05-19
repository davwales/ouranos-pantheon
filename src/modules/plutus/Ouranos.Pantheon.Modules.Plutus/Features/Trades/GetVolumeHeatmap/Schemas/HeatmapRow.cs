namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetVolumeHeatmap.Schemas;

internal sealed record HeatmapRow(int DayOfWeek, int Hour, long TotalTrades);

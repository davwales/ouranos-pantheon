namespace Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignalHistory.Schemas;

public sealed record SignalHistoryPoint(decimal Value, DateTimeOffset ComputedAt);

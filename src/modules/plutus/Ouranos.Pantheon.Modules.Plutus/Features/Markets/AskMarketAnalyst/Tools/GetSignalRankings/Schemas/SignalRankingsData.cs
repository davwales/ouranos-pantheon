namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Tools.GetSignalRankings.Schemas;

public sealed record SignalRankingsData(IReadOnlyList<SignalRankingData> Symbols);

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Tools.GetForecastLeaders.Schemas;

public sealed record ForecastLeadersData(IReadOnlyList<ForecastLeaderData> Symbols);

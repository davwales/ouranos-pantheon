namespace Ouranos.Pantheon.Plutus.Service.Application.Queries.Symbols.GetDailySymbolSummary;

public sealed record GetDailySymbolSummaryResponse(
    decimal AveragePrice,
    decimal MinPrice,
    decimal MaxPrice,
    decimal Volume
);
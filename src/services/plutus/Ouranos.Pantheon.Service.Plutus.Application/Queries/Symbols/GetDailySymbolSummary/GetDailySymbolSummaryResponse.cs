namespace Ouranos.Pantheon.Service.Plutus.Application.Queries.Symbols.GetDailySymbolSummary;

public sealed record GetDailySymbolSummaryResponse(
    decimal AveragePrice,
    decimal MinPrice,
    decimal MaxPrice,
    decimal Volume
);
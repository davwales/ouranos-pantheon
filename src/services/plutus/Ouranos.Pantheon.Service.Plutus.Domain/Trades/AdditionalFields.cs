namespace Ouranos.Pantheon.Service.Plutus.Domain.Trades;

public sealed record AdditionalFields(
    decimal? Limit = default,
    int? HighAlch = default,
    int? LowAlch = default,
    string? Exchange = default,
    string? Tape = default,
    string? ExternalTradeId = default
);
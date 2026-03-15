namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

public record AdditionalFields(
    decimal? Limit = null,
    int? HighAlch = null,
    int? LowAlch = null,
    string? Exchange = null,
    string? Tape = null,
    string? ExternalTradeId = null
);

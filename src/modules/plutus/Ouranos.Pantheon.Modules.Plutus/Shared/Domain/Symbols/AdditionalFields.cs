namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

public record AdditionalFields(
    decimal? Limit = null,
    decimal? HighAlch = null,
    decimal? LowAlch = null,
    string? Exchange = null,
    string? Tape = null,
    string? ExternalTradeId = null
);

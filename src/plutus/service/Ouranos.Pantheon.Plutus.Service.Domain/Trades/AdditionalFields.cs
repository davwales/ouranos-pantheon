namespace Ouranos.Pantheon.Plutus.Service.Domain.Trades;

public sealed record AdditionalFields(
    decimal? Limit = null,
    int? HighAlch = null,
    int? LowAlch = null,
    string? Exchange = null,
    string? Tape = null,
    string? ExternalTradeId = null
)
{
    private AdditionalFields() : this(
        null,
        null
    )
    {
    }
}
namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Shared;

public sealed record RabbitMqOptions(
    string Host,
    string Username,
    string Password,
    int? ConcurrencyLimit,
    int? RetryCount
)
{
    public RabbitMqOptions() : this(
        string.Empty,
        string.Empty,
        string.Empty,
        null,
        null
    )
    {
    }
}

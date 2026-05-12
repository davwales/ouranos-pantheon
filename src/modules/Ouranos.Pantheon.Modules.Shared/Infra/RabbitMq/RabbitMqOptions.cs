namespace Ouranos.Pantheon.Modules.Shared.Infra.RabbitMq;

public sealed record RabbitMqOptions(string Host, string Username, string Password, int? RetryCount)
{
    public const string SectionName = "Ouranos:RabbitMq";

    public RabbitMqOptions()
        : this(string.Empty, string.Empty, string.Empty, null) { }
}

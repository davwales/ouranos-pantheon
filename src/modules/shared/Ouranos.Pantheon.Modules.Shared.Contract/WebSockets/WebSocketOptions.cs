namespace Ouranos.Pantheon.Modules.Shared.Contract.WebSockets;

public sealed record WebSocketOptions(
    string Host,
    uint BufferSize,
    uint HealthCheckIntervalSeconds = 300,
    uint ReconnectBaseDelaySeconds = 5,
    uint ReconnectMaxDelaySeconds = 300
)
{
    public const string SectionName = "Ouranos:WebSocket";

    public WebSocketOptions()
        : this(string.Empty, 4096) { }
}

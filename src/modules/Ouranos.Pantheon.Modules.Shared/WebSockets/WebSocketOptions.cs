namespace Ouranos.Pantheon.Modules.Shared.WebSockets;

public sealed record WebSocketOptions(
    string Host,
    uint BufferSize,
    uint HealthCheckIntervalSeconds = 300,
    uint ErrorDelayIntervalSeconds = 300
)
{
    public const string SectionName = "Ouranos:WebSocket";

    public WebSocketOptions() : this(string.Empty, 4096)
    {
    }
}
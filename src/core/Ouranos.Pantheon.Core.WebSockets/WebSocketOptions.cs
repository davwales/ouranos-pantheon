namespace Ouranos.Pantheon.Core.WebSockets;

public sealed record WebSocketOptions(
    string Host,
    uint BufferSize
)
{
    public const string SectionName = "Ouranos:WebSocket";

    public WebSocketOptions() : this(string.Empty, 4096)
    {
    }
}
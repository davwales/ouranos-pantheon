using System.Collections.Concurrent;

namespace Ouranos.Pantheon.Modules.Shared.WebSockets;

public sealed class WebSocketHealthState
{
    private readonly ConcurrentDictionary<string, bool> _connections = new();

    public void Report(string workerName, bool isConnected)
    {
        _connections[workerName] = isConnected;
    }

    public IReadOnlyDictionary<string, bool> GetConnections()
    {
        return new Dictionary<string, bool>(_connections);
    }
}

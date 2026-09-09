using System.Collections.Concurrent;
using Ardalis.GuardClauses;

namespace Ouranos.Pantheon.Modules.Shared.Contract.WebSockets;

public sealed class WebSocketHealthState(WebSocketTelemetry telemetry)
{
    private readonly WebSocketTelemetry _telemetry = Guard.Against.Null(telemetry);
    private readonly ConcurrentDictionary<string, bool> _connections = new();

    public void Report(string workerName, bool isConnected)
    {
        var hadPrevious = _connections.TryGetValue(workerName, out var wasConnected);
        _connections[workerName] = isConnected;
        _telemetry.ReportConnection(workerName, hadPrevious ? wasConnected : null, isConnected);
    }

    public IReadOnlyDictionary<string, bool> GetConnections()
    {
        return new Dictionary<string, bool>(_connections);
    }
}

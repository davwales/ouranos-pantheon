using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Ouranos.Pantheon.Modules.Shared.Contract.WebSockets;

public sealed class WebSocketTelemetry : IDisposable
{
    public const string ActivitySourceName = "Ouranos.Pantheon.WebSockets";
    public const string MeterName = "Ouranos.Pantheon.WebSockets";

    internal const string HostTag = "websocket.host";
    internal const string WorkerNameTag = "websocket.worker_name";
    internal const string MessageTypeTag = "websocket.message_type";
    internal const string MessageSizeTag = "websocket.message_size_bytes";

    internal const string MessagesReceivedInstrumentName = "websocket.messages.received";
    internal const string MessagesSentInstrumentName = "websocket.messages.sent";
    internal const string DispatchDurationInstrumentName = "websocket.dispatch.duration";
    internal const string MessageSizeInstrumentName = "websocket.message.size";
    internal const string ActiveConnectionsInstrumentName = "websocket.connections.active";
    internal const string ReconnectsInstrumentName = "websocket.connections.reconnects";

    private readonly ActivitySource _activitySource = new(ActivitySourceName);
    private readonly Meter _meter = new(MeterName);

    private readonly Counter<long> _messagesReceived;
    private readonly Counter<long> _messagesSent;
    private readonly Histogram<double> _dispatchDuration;
    private readonly Histogram<long> _messageSizeBytes;
    private readonly UpDownCounter<long> _activeConnections;
    private readonly Counter<long> _reconnects;

    public WebSocketTelemetry()
    {
        _messagesReceived = _meter.CreateCounter<long>(
            MessagesReceivedInstrumentName,
            unit: "{message}",
            description: "Number of websocket messages received."
        );

        _messagesSent = _meter.CreateCounter<long>(
            MessagesSentInstrumentName,
            unit: "{message}",
            description: "Number of websocket messages sent to the remote host."
        );

        _dispatchDuration = _meter.CreateHistogram<double>(
            DispatchDurationInstrumentName,
            unit: "s",
            description: "Time to deserialize and dispatch a websocket message to all listeners."
        );

        _messageSizeBytes = _meter.CreateHistogram<long>(
            MessageSizeInstrumentName,
            unit: "By",
            description: "Payload size of websocket messages received."
        );

        _activeConnections = _meter.CreateUpDownCounter<long>(
            ActiveConnectionsInstrumentName,
            unit: "{connection}",
            description: "Number of websocket connections currently open, by worker."
        );

        _reconnects = _meter.CreateCounter<long>(
            ReconnectsInstrumentName,
            unit: "{reconnection}",
            description: "Number of successful reconnects, by worker."
        );
    }

    public WebSocketTelemetryScope Connect(string host)
    {
        var activity = _activitySource.StartActivity("websocket connect", ActivityKind.Client);
        activity?.SetTag(HostTag, host);
        return new WebSocketTelemetryScope(activity);
    }

    public WebSocketTelemetryScope Disconnect(string host)
    {
        var activity = _activitySource.StartActivity("websocket disconnect", ActivityKind.Client);
        activity?.SetTag(HostTag, host);
        return new WebSocketTelemetryScope(activity);
    }

    public WebSocketTelemetryScope MessageDispatch(string? host, int payloadSizeBytes)
    {
        // Message spans are trace roots by design: the listen loop runs inside the
        // connect span's ambient context, and without this every message of a session
        // would parent to it, producing one megatrace per connection.
        Activity.Current = null;

        var activity = _activitySource.StartActivity("websocket message", ActivityKind.Consumer);
        activity?.SetTag(HostTag, host);
        activity?.SetTag(MessageSizeTag, payloadSizeBytes);
        return new WebSocketTelemetryScope(activity);
    }

    public void RecordMessageSent(string host)
    {
        _messagesSent.Add(1, new KeyValuePair<string, object?>(HostTag, host));
    }

    public void ReportConnection(string workerName, bool? previous, bool isConnected)
    {
        if (previous == isConnected)
        {
            return;
        }

        if (previous is null && !isConnected)
        {
            return;
        }

        var tags = new KeyValuePair<string, object?>(WorkerNameTag, workerName);

        _activeConnections.Add(isConnected ? 1 : -1, tags);

        if (previous is false && isConnected)
        {
            _reconnects.Add(1, tags);
        }
    }

    public void Dispose()
    {
        _meter.Dispose();
        _activitySource.Dispose();
    }

    internal void RecordMessageReceived(string? host, int payloadSizeBytes, TimeSpan duration)
    {
        KeyValuePair<string, object?>[] tags = host is null
            ? []
            : [new KeyValuePair<string, object?>(HostTag, host)];

        _messagesReceived.Add(1, tags);
        _messageSizeBytes.Record(payloadSizeBytes, tags);
        _dispatchDuration.Record(duration.TotalSeconds, tags);
    }
}

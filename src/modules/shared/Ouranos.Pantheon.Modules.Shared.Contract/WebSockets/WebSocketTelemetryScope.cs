using System.Diagnostics;

namespace Ouranos.Pantheon.Modules.Shared.Contract.WebSockets;

/// <summary>
/// Disposable lifespan for one websocket telemetry span. Owns the activity from
/// creation until disposal; <see cref="Fail"/> marks the span failed when called
/// from a catch block.
/// </summary>
public readonly struct WebSocketTelemetryScope : IDisposable
{
    private readonly Activity? _activity;

    internal WebSocketTelemetryScope(Activity? activity)
    {
        _activity = activity;
    }

    public void SetMessageType(string messageTypeName)
    {
        _activity?.SetTag(WebSocketTelemetry.MessageTypeTag, messageTypeName);
    }

    public void Fail(Exception exception)
    {
        if (_activity is null)
        {
            return;
        }

        _activity.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
        _activity.AddException(exception);
    }

    public void Dispose()
    {
        _activity?.Dispose();
    }
}

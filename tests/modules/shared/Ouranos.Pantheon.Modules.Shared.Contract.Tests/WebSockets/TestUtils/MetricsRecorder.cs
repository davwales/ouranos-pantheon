using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Tests.WebSockets.TestUtils;

public sealed class MetricsRecorder : IDisposable
{
    private readonly MeterListener _listener;

    public ConcurrentQueue<(
        string Instrument,
        long Value,
        KeyValuePair<string, object?>[] Tags
    )> LongMeasurements { get; } = new();

    public ConcurrentQueue<(
        string Instrument,
        double Value,
        KeyValuePair<string, object?>[] Tags
    )> DoubleMeasurements { get; } = new();

    public MetricsRecorder()
    {
        _listener = new MeterListener
        {
            InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == WebSocketTelemetry.MeterName)
                {
                    listener.EnableMeasurementEvents(instrument);
                }
            },
        };
        _listener.SetMeasurementEventCallback<long>(RecordLongMeasurement);
        _listener.SetMeasurementEventCallback<double>(RecordDoubleMeasurement);
        _listener.Start();
    }

    public void Dispose()
    {
        _listener.Dispose();
    }

    private void RecordLongMeasurement(
        Instrument instrument,
        long value,
        ReadOnlySpan<KeyValuePair<string, object?>> tags,
        object? state
    )
    {
        LongMeasurements.Enqueue((instrument.Name, value, [.. tags]));
    }

    private void RecordDoubleMeasurement(
        Instrument instrument,
        double value,
        ReadOnlySpan<KeyValuePair<string, object?>> tags,
        object? state
    )
    {
        DoubleMeasurements.Enqueue((instrument.Name, value, [.. tags]));
    }
}

using Ouranos.Pantheon.Modules.Shared.Contract.Tests.WebSockets.TestUtils;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Tests.WebSockets;

public sealed class WebSocketHealthStateTests
{
    private const string TestWorker = "telemetry-test-worker";

    private static bool HasTag(KeyValuePair<string, object?>[] tags, string key, object value)
    {
        return tags.Any(tag => tag.Key == key && tag.Value?.ToString() == value.ToString());
    }

    [Fact]
    public void Report_WhenWorkerConnects_ShouldIncrementActiveConnections()
    {
        // Arrange
        var health = new WebSocketHealthState(new WebSocketTelemetry());
        using var recorder = new MetricsRecorder();

        // Act
        health.Report(TestWorker, true);

        // Assert
        var gauge = recorder
            .LongMeasurements.Single(m =>
                m.Instrument == WebSocketTelemetry.ActiveConnectionsInstrumentName
                && HasTag(m.Tags, WebSocketTelemetry.WorkerNameTag, TestWorker)
            )
            .Value;
        gauge.ShouldBe(1);
    }

    [Fact]
    public void Report_WhenWorkerReconnects_ShouldCountReconnectAndTrackGauge()
    {
        // Arrange
        var health = new WebSocketHealthState(new WebSocketTelemetry());
        health.Report(TestWorker, true);
        using var recorder = new MetricsRecorder();

        // Act
        health.Report(TestWorker, false);
        health.Report(TestWorker, true);

        // Assert
        var gaugeValues = recorder
            .LongMeasurements.Where(m =>
                m.Instrument == WebSocketTelemetry.ActiveConnectionsInstrumentName
                && HasTag(m.Tags, WebSocketTelemetry.WorkerNameTag, TestWorker)
            )
            .Select(m => m.Value)
            .ToList();
        gaugeValues.ShouldBe([-1, 1]);

        var reconnects = recorder
            .LongMeasurements.Where(m =>
                m.Instrument == WebSocketTelemetry.ReconnectsInstrumentName
                && HasTag(m.Tags, WebSocketTelemetry.WorkerNameTag, TestWorker)
            )
            .Select(m => m.Value)
            .ToList();
        reconnects.ShouldBe([1]);
    }

    [Fact]
    public void Report_WhenDuplicateDisconnectReported_ShouldNotDoubleCountGauge()
    {
        // Arrange
        var health = new WebSocketHealthState(new WebSocketTelemetry());
        health.Report(TestWorker, false);
        using var recorder = new MetricsRecorder();

        // Act
        health.Report(TestWorker, false);

        // Assert
        recorder
            .LongMeasurements.Where(m =>
                m.Instrument == WebSocketTelemetry.ActiveConnectionsInstrumentName
                && HasTag(m.Tags, WebSocketTelemetry.WorkerNameTag, TestWorker)
            )
            .ShouldBeEmpty();
    }

    [Fact]
    public void Report_WhenFirstReportIsDisconnected_ShouldNotDecrementGauge()
    {
        // Arrange
        var health = new WebSocketHealthState(new WebSocketTelemetry());
        using var recorder = new MetricsRecorder();

        // Act
        health.Report(TestWorker, false);

        // Assert
        recorder
            .LongMeasurements.Where(m =>
                m.Instrument == WebSocketTelemetry.ActiveConnectionsInstrumentName
                && HasTag(m.Tags, WebSocketTelemetry.WorkerNameTag, TestWorker)
            )
            .ShouldBeEmpty();
    }
}

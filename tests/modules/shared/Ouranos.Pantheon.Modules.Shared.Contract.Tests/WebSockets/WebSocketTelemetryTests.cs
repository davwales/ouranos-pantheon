using System.Diagnostics;
using NSubstitute.ExceptionExtensions;
using Ouranos.Pantheon.Modules.Shared.Contract.Tests.WebSockets.TestUtils;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.Listeners;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.Serializers;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.WebSocketClients;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Tests.WebSockets;

public sealed class WebSocketTelemetryTests
{
    private const string TestHost = "wss://telemetry-test.local";

    private static IWebSocketClient GivenClient()
    {
        var client = Substitute.For<IWebSocketClient>();
        client.Host.Returns(TestHost);
        return client;
    }

    private static ListenerRegistry GivenRegistry(IMessageSerializer serializer)
    {
        return new ListenerRegistry(serializer, new WebSocketTelemetry());
    }

    private static object? GetTagValue(Activity activity, string key)
    {
        return activity.TagObjects.FirstOrDefault(tag => tag.Key == key).Value;
    }

    private static bool HasTag(KeyValuePair<string, object?>[] tags, string key, object value)
    {
        return tags.Any(tag => tag.Key == key && tag.Value?.ToString() == value.ToString());
    }

    private static bool IsTestHostActivity(Activity activity)
    {
        return GetTagValue(activity, WebSocketTelemetry.HostTag)?.ToString() == TestHost;
    }

    [Fact]
    public void Connect_WhenCalled_ShouldStartConnectSpan()
    {
        // Arrange
        using var telemetry = new WebSocketTelemetry();
        using var recorder = new ActivityRecorder();

        // Act
        using (var scope = telemetry.Connect(TestHost)) { }

        // Assert
        var span = recorder.Activities.Single(IsTestHostActivity);
        span.OperationName.ShouldBe("websocket connect");
        span.Kind.ShouldBe(ActivityKind.Client);
    }

    [Fact]
    public void Disconnect_WhenCalled_ShouldStartDisconnectSpan()
    {
        // Arrange
        using var telemetry = new WebSocketTelemetry();
        using var recorder = new ActivityRecorder();

        // Act
        using (var scope = telemetry.Disconnect(TestHost)) { }

        // Assert
        var span = recorder.Activities.Single(IsTestHostActivity);
        span.OperationName.ShouldBe("websocket disconnect");
        span.Kind.ShouldBe(ActivityKind.Client);
    }

    [Fact]
    public void Scope_WhenFailCalled_ShouldMarkSpanFailed()
    {
        // Arrange
        using var telemetry = new WebSocketTelemetry();
        using var recorder = new ActivityRecorder();
        var exception = new InvalidOperationException("boom");

        // Act
        using (var scope = telemetry.Connect(TestHost))
        {
            scope.Fail(exception);
        }

        // Assert
        var span = recorder.Activities.Single(IsTestHostActivity);
        span.Status.ShouldBe(ActivityStatusCode.Error);
        span.Events.ShouldContain(e => e.Name == "exception");
    }

    [Fact]
    public async Task HandleMessageAsync_WhenMessageDispatched_ShouldStartMessageSpan()
    {
        // Arrange
        var fixture = new Fixture();
        var bytes = fixture.Create<byte[]>();
        var entity = fixture.Create<TestEntity>();
        var serializer = Substitute.For<IMessageSerializer>();
        serializer.Deserialize<object>(bytes).Returns(entity);
        var registry = GivenRegistry(serializer);
        var listener = Substitute.For<IListener<TestEntity>>();
        registry.RegisterListener(listener);
        var client = GivenClient();
        using var recorder = new ActivityRecorder();

        // Act
        await registry.HandleMessageAsync(bytes, client, CancellationToken.None);

        // Assert
        var span = recorder.Activities.Single(IsTestHostActivity);
        span.OperationName.ShouldBe("websocket message");
        span.Kind.ShouldBe(ActivityKind.Consumer);
        GetTagValue(span, WebSocketTelemetry.MessageTypeTag).ShouldBe("TestEntity");
        GetTagValue(span, WebSocketTelemetry.MessageSizeTag).ShouldBe(bytes.Length);
    }

    [Fact]
    public async Task HandleMessageAsync_WhenAmbientActivityPresent_ShouldStartMessageSpanAsRoot()
    {
        // Arrange
        var fixture = new Fixture();
        var bytes = fixture.Create<byte[]>();
        var entity = fixture.Create<TestEntity>();
        var serializer = Substitute.For<IMessageSerializer>();
        serializer.Deserialize<object>(bytes).Returns(entity);
        var registry = GivenRegistry(serializer);
        var listener = Substitute.For<IListener<TestEntity>>();
        registry.RegisterListener(listener);
        var client = GivenClient();
        using var recorder = new ActivityRecorder();
        using var ambient = new Activity("ambient parent").Start();

        // Act
        await registry.HandleMessageAsync(bytes, client, CancellationToken.None);

        // Assert
        var span = recorder.Activities.Single(IsTestHostActivity);
        span.ParentId.ShouldBeNull();
    }

    [Fact]
    public async Task HandleMessageAsync_WhenListenerThrows_ShouldMarkSpanFailed()
    {
        // Arrange
        var fixture = new Fixture();
        var bytes = fixture.Create<byte[]>();
        var entity = fixture.Create<TestEntity>();
        var serializer = Substitute.For<IMessageSerializer>();
        serializer.Deserialize<object>(bytes).Returns(entity);
        var registry = GivenRegistry(serializer);
        var listener = Substitute.For<IListener<TestEntity>>();
        listener
            .HandleMessageAsync(
                Arg.Any<TestEntity>(),
                Arg.Any<IWebSocketClient>(),
                Arg.Any<CancellationToken>()
            )
            .ThrowsAsync(new InvalidOperationException("boom"));
        registry.RegisterListener(listener);
        var client = GivenClient();
        using var recorder = new ActivityRecorder();

        // Act
        await registry
            .HandleMessageAsync(bytes, client, CancellationToken.None)
            .ShouldThrowAsync<InvalidOperationException>();

        // Assert
        var span = recorder.Activities.Single(IsTestHostActivity);
        span.Status.ShouldBe(ActivityStatusCode.Error);
        span.Events.ShouldContain(e => e.Name == "exception");
    }

    [Fact]
    public async Task HandleMessageAsync_WhenMessageDispatched_ShouldRecordMessageMetrics()
    {
        // Arrange
        var fixture = new Fixture();
        var bytes = fixture.Create<byte[]>();
        var entity = fixture.Create<TestEntity>();
        var serializer = Substitute.For<IMessageSerializer>();
        serializer.Deserialize<object>(bytes).Returns(entity);
        var registry = GivenRegistry(serializer);
        var listener = Substitute.For<IListener<TestEntity>>();
        registry.RegisterListener(listener);
        var client = GivenClient();
        using var recorder = new MetricsRecorder();

        // Act
        await registry.HandleMessageAsync(bytes, client, CancellationToken.None);

        // Assert
        var received = recorder
            .LongMeasurements.Single(m =>
                m.Instrument == WebSocketTelemetry.MessagesReceivedInstrumentName
                && HasTag(m.Tags, WebSocketTelemetry.HostTag, TestHost)
            )
            .Value;
        received.ShouldBe(1);

        var size = recorder
            .LongMeasurements.Single(m =>
                m.Instrument == WebSocketTelemetry.MessageSizeInstrumentName
                && HasTag(m.Tags, WebSocketTelemetry.HostTag, TestHost)
            )
            .Value;
        size.ShouldBe(bytes.Length);

        var duration = recorder
            .DoubleMeasurements.Single(m =>
                m.Instrument == WebSocketTelemetry.DispatchDurationInstrumentName
                && HasTag(m.Tags, WebSocketTelemetry.HostTag, TestHost)
            )
            .Value;
        duration.ShouldBeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task HandleMessageAsync_WhenListenerThrows_ShouldStillRecordMessageMetrics()
    {
        // Arrange
        var fixture = new Fixture();
        var bytes = fixture.Create<byte[]>();
        var entity = fixture.Create<TestEntity>();
        var serializer = Substitute.For<IMessageSerializer>();
        serializer.Deserialize<object>(bytes).Returns(entity);
        var registry = GivenRegistry(serializer);
        var listener = Substitute.For<IListener<TestEntity>>();

        listener
            .HandleMessageAsync(
                Arg.Any<TestEntity>(),
                Arg.Any<IWebSocketClient>(),
                Arg.Any<CancellationToken>()
            )
            .ThrowsAsync(new InvalidOperationException("boom"));

        registry.RegisterListener(listener);
        var client = GivenClient();
        using var recorder = new MetricsRecorder();

        // Act
        await registry
            .HandleMessageAsync(bytes, client, CancellationToken.None)
            .ShouldThrowAsync<InvalidOperationException>();

        // Assert
        var received = recorder
            .LongMeasurements.Single(m =>
                m.Instrument == WebSocketTelemetry.MessagesReceivedInstrumentName
                && HasTag(m.Tags, WebSocketTelemetry.HostTag, TestHost)
            )
            .Value;
        received.ShouldBe(1);

        recorder
            .LongMeasurements.Where(m =>
                m.Instrument == WebSocketTelemetry.MessageSizeInstrumentName
                && HasTag(m.Tags, WebSocketTelemetry.HostTag, TestHost)
            )
            .ShouldNotBeEmpty();

        recorder
            .DoubleMeasurements.Where(m =>
                m.Instrument == WebSocketTelemetry.DispatchDurationInstrumentName
                && HasTag(m.Tags, WebSocketTelemetry.HostTag, TestHost)
            )
            .ShouldNotBeEmpty();
    }

    [Fact]
    public void RecordMessageSent_WhenCalled_ShouldCountSentMessage()
    {
        // Arrange
        using var telemetry = new WebSocketTelemetry();
        using var recorder = new MetricsRecorder();

        // Act
        telemetry.RecordMessageSent(TestHost);

        // Assert
        var sent = recorder
            .LongMeasurements.Single(m =>
                m.Instrument == WebSocketTelemetry.MessagesSentInstrumentName
                && HasTag(m.Tags, WebSocketTelemetry.HostTag, TestHost)
            )
            .Value;
        sent.ShouldBe(1);
    }
}

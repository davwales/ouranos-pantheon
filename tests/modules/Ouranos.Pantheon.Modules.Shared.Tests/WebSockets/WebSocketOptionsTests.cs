using Ouranos.Pantheon.Modules.Shared.WebSockets;

namespace Ouranos.Pantheon.Modules.Shared.Tests.WebSockets;

public sealed class WebSocketOptionsTests
{
    [Fact]
    public void SectionName_ShouldBeExpectedValue()
    {
        // Assert
        WebSocketOptions.SectionName.ShouldBe("Ouranos:WebSocket");
    }

    [Fact]
    public void Constructor_WhenGivenOptionalParameters_ShouldSetExpectedValues()
    {
        // Arrange
        var fixture = new Fixture();
        var expectedHost = fixture.Create<string>();
        var expectedBufferSize = fixture.Create<uint>();
        var expectedHealthCheckIntervalSeconds = fixture.Create<uint>();
        var expectedReconnectBaseDelaySeconds = fixture.Create<uint>();
        var expectedReconnectMaxDelaySeconds = fixture.Create<uint>();

        // Act
        var options = new WebSocketOptions(
            expectedHost,
            expectedBufferSize,
            expectedHealthCheckIntervalSeconds,
            expectedReconnectBaseDelaySeconds,
            expectedReconnectMaxDelaySeconds
        );

        // Assert
        options.Host.ShouldBe(expectedHost);
        options.BufferSize.ShouldBe(expectedBufferSize);
        options.HealthCheckIntervalSeconds.ShouldBe(expectedHealthCheckIntervalSeconds);
        options.ReconnectBaseDelaySeconds.ShouldBe(expectedReconnectBaseDelaySeconds);
        options.ReconnectMaxDelaySeconds.ShouldBe(expectedReconnectMaxDelaySeconds);
    }

    [Fact]
    public void Constructor_WhenNotGivenOptionalParameters_ShouldSetExpectedValues()
    {
        // Arrange
        var fixture = new Fixture();
        var expectedHost = fixture.Create<string>();
        var expectedBufferSize = fixture.Create<uint>();

        // Act
        var options = new WebSocketOptions(
            expectedHost,
            expectedBufferSize
        );

        // Assert
        options.Host.ShouldBe(expectedHost);
        options.BufferSize.ShouldBe(expectedBufferSize);
        options.HealthCheckIntervalSeconds.ShouldBe((uint)300);
        options.ReconnectBaseDelaySeconds.ShouldBe((uint)5);
        options.ReconnectMaxDelaySeconds.ShouldBe((uint)300);
    }
}

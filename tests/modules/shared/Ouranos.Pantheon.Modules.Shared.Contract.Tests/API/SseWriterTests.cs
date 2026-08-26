using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Shared.Contract.API;

namespace Ouranos.Pantheon.Modules.Shared.Tests.API;

public sealed class SseWriterTests
{
    [Fact]
    public void SetSseHeaders_WhenCalled_ShouldSetContentTypeCacheControlAndConnectionHeaders()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var response = httpContext.Response;

        // Act
        SseWriter.SetSseHeaders(response);

        // Assert
        response.Headers.ContentType.ToString().ShouldBe("text/event-stream");
        response.Headers.CacheControl.ToString().ShouldBe("no-cache");
        response.Headers.Connection.ToString().ShouldBe("keep-alive");
    }

    [Fact]
    public async Task WriteEventAsync_WhenCalled_ShouldWriteDataInSseFormat()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var response = httpContext.Response;
        response.Body = new MemoryStream();
        var data = new { Message = "hello", Count = 42 };

        // Act
        await SseWriter.WriteEventAsync(response, data);

        // Assert
        response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(response.Body);
        var output = await reader.ReadToEndAsync();
        output.ShouldStartWith("data: ");
        output.ShouldEndWith("\n\n");
        output.ShouldMatch("data: \\{.*\"message\".*\"hello\".*\"count\".*42.*\\}\n\n");
    }

    [Fact]
    public async Task WriteEventAsync_WhenCalled_ShouldSerializeDataWithCamelCase()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var response = httpContext.Response;
        response.Body = new MemoryStream();
        var data = new TestPerson(Age: 30, FirstName: "Alice");

        // Act
        await SseWriter.WriteEventAsync(response, data);

        // Assert
        response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(response.Body);
        var output = await reader.ReadToEndAsync();
        output.ShouldContain("\"firstName\"");
        output.ShouldContain("\"age\"");
        output.ShouldNotContain("\"FirstName\"", Case.Sensitive);
        output.ShouldNotContain("\"Age\"", Case.Sensitive);
    }

    [Fact]
    public async Task WriteEventAsync_WhenCalled_ShouldFlushResponseBody()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var response = httpContext.Response;
        var stream = new MemoryStream();
        response.Body = stream;
        var data = new { Value = 123 };

        // Act
        await SseWriter.WriteEventAsync(response, data);

        // Assert
        stream.Length.ShouldBeGreaterThan(0);
        stream.Position.ShouldBe(stream.Length);
    }

    [Fact]
    public async Task WriteEventAsync_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var response = httpContext.Response;
        response.Body = new MemoryStream();
        var data = new { Value = 123 };
        var cancellationToken = new CancellationToken(canceled: true);

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
        {
            await SseWriter.WriteEventAsync(response, data, cancellationToken);
        });
    }

    private sealed record TestPerson(int Age, string FirstName = "");
}

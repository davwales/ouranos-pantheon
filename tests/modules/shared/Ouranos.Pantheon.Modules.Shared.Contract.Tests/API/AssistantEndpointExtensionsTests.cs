using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Shared.Contract.API;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Tests.API;

public sealed class AssistantEndpointExtensionsTests
{
    public sealed record TestContext(string Topic);

    private static async IAsyncEnumerable<AssistantEvent> CreateEvents()
    {
        await Task.Yield();
        yield return new AssistantReasoningEvent("Hmm");
        yield return new AssistantContentEvent("Hello");
        yield return new AssistantDoneEvent();
    }

    [Fact]
    public void MapAssistant_WhenCalled_ShouldRegisterPostRoute()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions { EnvironmentName = "Testing" }
        );

        builder.Services.AddSingleton(Substitute.For<IMessageBus>());
        var app = builder.Build();

        // Act
        app.MapAssistant<TestContext>("/api/test/assistant/completions/stream");

        // Assert
        var endpoint = ((IEndpointRouteBuilder)app)
            .DataSources.SelectMany(d => d.Endpoints)
            .OfType<RouteEndpoint>()
            .Single();

        endpoint.RoutePattern.RawText.ShouldBe("/api/test/assistant/completions/stream");
        endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.ShouldBe(["POST"]);
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldWriteEachEventAsTypedSse()
    {
        // Arrange
        var input = new AssistantCompletionInput<TestContext>([], new TestContext("baking"));
        var bus = Substitute.For<IMessageBus>();

        bus.InvokeAsync<IAsyncEnumerable<AssistantEvent>>(
                input,
                Arg.Any<CancellationToken>(),
                Arg.Any<TimeSpan?>()
            )
            .Returns(CreateEvents());

        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        // Act
        await AssistantEndpointExtensions.HandleAsync(
            input,
            bus,
            httpContext,
            CancellationToken.None
        );

        // Assert
        httpContext.Response.Headers.ContentType.ToString().ShouldBe("text/event-stream");
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(httpContext.Response.Body);
        var output = await reader.ReadToEndAsync();
        output.ShouldBe(
            "data: {\"$type\":\"reasoning\",\"content\":\"Hmm\"}\n\n"
                + "data: {\"$type\":\"content\",\"content\":\"Hello\"}\n\n"
                + "data: {\"$type\":\"done\"}\n\n"
        );
    }
}

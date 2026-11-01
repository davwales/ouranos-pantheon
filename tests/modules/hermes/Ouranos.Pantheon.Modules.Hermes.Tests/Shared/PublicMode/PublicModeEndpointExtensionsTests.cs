using System.Text;
using Flagsmith;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.PublicMode;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Shared.PublicMode;

public sealed class PublicModeEndpointExtensionsTests
{
    private readonly IFlagsmithClient _flagsmith = Substitute.For<IFlagsmithClient>();
    private readonly WebApplication _app;

    public PublicModeEndpointExtensionsTests()
    {
        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions { EnvironmentName = "Testing" }
        );
        builder.Services.AddSingleton(_flagsmith);
        _app = builder.Build();

        var flags = Substitute.For<IFlags>();
        flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode).Returns(Task.FromResult(true));
        _flagsmith.GetEnvironmentFlags().Returns(Task.FromResult(flags));
    }

    public sealed record WriteBody(bool Writes);

    private RouteEndpoint SingleEndpoint()
    {
        return ((IEndpointRouteBuilder)_app)
            .DataSources.SelectMany(d => d.Endpoints)
            .OfType<RouteEndpoint>()
            .Single();
    }

    private async Task<int> SendAsync(RouteEndpoint endpoint, string body)
    {
        var context = new DefaultHttpContext { RequestServices = _app.Services };
        context.Request.Method = HttpMethods.Post;
        context.Request.ContentType = "application/json";
        var bytes = Encoding.UTF8.GetBytes(body);
        context.Request.Body = new MemoryStream(bytes);
        context.Request.ContentLength = bytes.Length;
        var bodyDetection = Substitute.For<IHttpRequestBodyDetectionFeature>();
        bodyDetection.CanHaveBody.Returns(true);
        context.Features.Set(bodyDetection);
        context.Response.Body = new MemoryStream();

        endpoint.RequestDelegate.ShouldNotBeNull();
        await endpoint.RequestDelegate(context);

        return context.Response.StatusCode;
    }

    [Fact]
    public async Task DisabledInPublicMode_ShouldMarkEndpointAndRejectInPublicMode()
    {
        // Arrange
        _app.MapPost("/write", () => Results.Ok()).DisabledInPublicMode();
        var endpoint = SingleEndpoint();

        // Act
        var status = await SendAsync(endpoint, "{}");

        // Assert
        endpoint.Metadata.GetMetadata<PublicModeWriteMetadata>().ShouldNotBeNull();
        status.ShouldBe(StatusCodes.Status403Forbidden);
    }

    [Theory]
    [InlineData(true, StatusCodes.Status403Forbidden)]
    [InlineData(false, StatusCodes.Status200OK)]
    public async Task DisabledInPublicMode_WithPredicate_ShouldRejectOnlyMatchingRequests(
        bool writes,
        int expectedStatus
    )
    {
        // Arrange
        _app.MapPost("/maybe-write", (WriteBody body) => Results.Ok())
            .DisabledInPublicMode<WriteBody>(body => body.Writes);
        var endpoint = SingleEndpoint();

        // Act
        var status = await SendAsync(endpoint, $$"""{"writes":{{(writes ? "true" : "false")}}}""");

        // Assert
        endpoint.Metadata.GetMetadata<PublicModeWriteMetadata>().ShouldNotBeNull();
        status.ShouldBe(expectedStatus);
    }
}

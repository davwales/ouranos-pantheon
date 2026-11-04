using Flagsmith;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.PublicMode;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Shared.PublicMode;

public sealed class PublicModeWriteFilterTests
{
    private readonly IFlagsmithClient _flagsmith = Substitute.For<IFlagsmithClient>();
    private readonly EndpointFilterInvocationContext _context;
    private readonly object _nextResult = new();
    private bool _nextCalled;

    public PublicModeWriteFilterTests()
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection()
                .AddSingleton(_flagsmith)
                .BuildServiceProvider(),
        };
        _context = new DefaultEndpointFilterInvocationContext(httpContext);
    }

    private void SetupPublicMode(bool enabled)
    {
        var flags = Substitute.For<IFlags>();
        flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode).Returns(Task.FromResult(enabled));
        _flagsmith.GetEnvironmentFlags().Returns(Task.FromResult(flags));
    }

    private ValueTask<object?> Next(EndpointFilterInvocationContext context)
    {
        _nextCalled = true;
        return ValueTask.FromResult<object?>(_nextResult);
    }

    [Fact]
    public void Constructor_WhenWritesIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new PublicModeWriteFilter(null!));
    }

    [Fact]
    public async Task InvokeAsync_WhenPublicModeAndWrite_ShouldReturnForbidden()
    {
        // Arrange
        SetupPublicMode(true);
        var filter = new PublicModeWriteFilter(_ => true);

        // Act
        var result = await filter.InvokeAsync(_context, Next);

        // Assert
        var problem = result.ShouldBeOfType<ProblemHttpResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status403Forbidden);
        problem.ProblemDetails.Detail.ShouldBe(PublicModeWriteFilter.Detail);
        _nextCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task InvokeAsync_WhenPublicModeOff_ShouldCallNext()
    {
        // Arrange
        SetupPublicMode(false);
        var filter = new PublicModeWriteFilter(_ => true);

        // Act
        var result = await filter.InvokeAsync(_context, Next);

        // Assert
        result.ShouldBeSameAs(_nextResult);
        _nextCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhenRequestDoesNotWrite_ShouldCallNextWithoutReadingFlags()
    {
        // Arrange
        SetupPublicMode(true);
        var filter = new PublicModeWriteFilter(_ => false);

        // Act
        var result = await filter.InvokeAsync(_context, Next);

        // Assert
        result.ShouldBeSameAs(_nextResult);
        await _flagsmith.DidNotReceive().GetEnvironmentFlags();
    }
}

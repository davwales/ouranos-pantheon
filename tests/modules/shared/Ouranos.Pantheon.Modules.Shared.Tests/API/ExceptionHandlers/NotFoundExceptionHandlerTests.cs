using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Shared.API.ExceptionHandlers;

namespace Ouranos.Pantheon.Modules.Shared.Tests.API.ExceptionHandlers;

public sealed class NotFoundExceptionHandlerTests
{
    private readonly IProblemDetailsService _problemDetailsService =
        Substitute.For<IProblemDetailsService>();
    private readonly NotFoundExceptionHandler _handler;

    public NotFoundExceptionHandlerTests()
    {
        _handler = new NotFoundExceptionHandler(_problemDetailsService);
    }

    [Fact]
    public async Task TryHandleAsync_WhenExceptionIsNotNotFound_ShouldReturnFalse()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();

        // Act
        var handled = await _handler.TryHandleAsync(
            httpContext,
            new InvalidOperationException("boom"),
            CancellationToken.None
        );

        // Assert
        handled.ShouldBeFalse();
        await _problemDetailsService
            .DidNotReceive()
            .TryWriteAsync(Arg.Any<ProblemDetailsContext>());
    }

    [Fact]
    public async Task TryHandleAsync_WhenNotFound_ShouldWriteNotFoundProblemDetails()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var exception = new NotFoundException("backtest-1", "Backtest");
        _problemDetailsService.TryWriteAsync(Arg.Any<ProblemDetailsContext>()).Returns(true);

        // Act
        var handled = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        handled.ShouldBeTrue();
        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status404NotFound);

        await _problemDetailsService
            .Received(1)
            .TryWriteAsync(
                Arg.Is<ProblemDetailsContext>(c =>
                    c.ProblemDetails.Status == StatusCodes.Status404NotFound
                    && c.ProblemDetails.Detail == exception.Message
                )
            );
    }
}

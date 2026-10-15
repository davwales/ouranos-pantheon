using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Shared.API.ExceptionHandlers;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Filtering;

namespace Ouranos.Pantheon.Modules.Shared.Tests.API.ExceptionHandlers;

public sealed class InvalidFilterExceptionHandlerTests
{
    private readonly IProblemDetailsService _problemDetailsService =
        Substitute.For<IProblemDetailsService>();
    private readonly InvalidFilterExceptionHandler _handler;

    public InvalidFilterExceptionHandlerTests()
    {
        _handler = new InvalidFilterExceptionHandler(_problemDetailsService);
    }

    [Fact]
    public void Constructor_WhenProblemDetailsServiceIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new InvalidFilterExceptionHandler(null!));
    }

    [Fact]
    public async Task TryHandleAsync_WhenExceptionIsNotInvalidFilter_ShouldReturnFalse()
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
        await _problemDetailsService.DidNotReceiveWithAnyArgs().TryWriteAsync(default!);
    }

    [Fact]
    public async Task TryHandleAsync_WhenInvalidFilter_ShouldWriteBadRequestProblemDetails()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var exception = new InvalidFilterException("Invalid filter 'x': unknown field");
        _problemDetailsService.TryWriteAsync(Arg.Any<ProblemDetailsContext>()).Returns(true);

        // Act
        var handled = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        handled.ShouldBeTrue();
        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);

        await _problemDetailsService
            .Received(1)
            .TryWriteAsync(
                Arg.Is<ProblemDetailsContext>(c =>
                    c.ProblemDetails.Status == StatusCodes.Status400BadRequest
                    && c.ProblemDetails.Detail == exception.Message
                )
            );
    }
}

using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Shared.API.ExceptionHandlers;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;

namespace Ouranos.Pantheon.Modules.Shared.Tests.API.ExceptionHandlers;

public sealed class BadRequestExceptionHandlerTests
{
    private readonly IProblemDetailsService _problemDetailsService =
        Substitute.For<IProblemDetailsService>();
    private readonly BadRequestExceptionHandler _handler;

    public BadRequestExceptionHandlerTests()
    {
        _handler = new BadRequestExceptionHandler(_problemDetailsService);
    }

    [Fact]
    public void Constructor_WhenProblemDetailsServiceIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new BadRequestExceptionHandler(null!));
    }

    [Fact]
    public async Task TryHandleAsync_WhenExceptionIsNotBadRequest_ShouldReturnFalse()
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
    public async Task TryHandleAsync_WhenBadRequest_ShouldWriteBadRequestProblemDetails()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var exception = new BadRequestException("The symbol does not belong to the market.");
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

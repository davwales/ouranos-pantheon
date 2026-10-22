using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Shared.API.ExceptionHandlers;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;

namespace Ouranos.Pantheon.Modules.Shared.Tests.API.ExceptionHandlers;

public sealed class ConflictExceptionHandlerTests
{
    private readonly IProblemDetailsService _problemDetailsService =
        Substitute.For<IProblemDetailsService>();
    private readonly ConflictExceptionHandler _handler;

    public ConflictExceptionHandlerTests()
    {
        _handler = new ConflictExceptionHandler(_problemDetailsService);
    }

    [Fact]
    public void Constructor_WhenProblemDetailsServiceIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new ConflictExceptionHandler(null!));
    }

    [Fact]
    public async Task TryHandleAsync_WhenExceptionIsNotConflict_ShouldReturnFalse()
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
    public async Task TryHandleAsync_WhenConflict_ShouldWriteConflictProblemDetails()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var exception = new ConflictException("The recipe is already being imported.");
        _problemDetailsService.TryWriteAsync(Arg.Any<ProblemDetailsContext>()).Returns(true);

        // Act
        var handled = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        handled.ShouldBeTrue();
        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status409Conflict);

        await _problemDetailsService
            .Received(1)
            .TryWriteAsync(
                Arg.Is<ProblemDetailsContext>(c =>
                    c.ProblemDetails.Status == StatusCodes.Status409Conflict
                    && c.ProblemDetails.Detail == exception.Message
                )
            );
    }
}

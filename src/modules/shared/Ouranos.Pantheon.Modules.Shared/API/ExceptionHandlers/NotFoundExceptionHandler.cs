using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ouranos.Pantheon.Modules.Shared.API.ExceptionHandlers;

public sealed class NotFoundExceptionHandler(IProblemDetailsService problemDetailsService)
    : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService = Guard.Against.Null(
        problemDetailsService
    );

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        if (exception is not NotFoundException notFound)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;

        return await _problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Not Found",
                    Detail = notFound.Message,
                },
            }
        );
    }
}

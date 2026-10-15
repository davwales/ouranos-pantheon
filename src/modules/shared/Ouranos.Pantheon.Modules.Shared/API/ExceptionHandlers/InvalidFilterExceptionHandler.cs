using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Filtering;

namespace Ouranos.Pantheon.Modules.Shared.API.ExceptionHandlers;

public sealed class InvalidFilterExceptionHandler(IProblemDetailsService problemDetailsService)
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
        if (exception is not InvalidFilterException invalidFilter)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        return await _problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid filter",
                    Detail = invalidFilter.Message,
                },
            }
        );
    }
}

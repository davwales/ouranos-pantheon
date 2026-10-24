using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Shared.Features.Health.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Shared.Features.Health;

public static class HealthEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/health", Handle)
            .WithTags("Health")
            .AllowAnonymous()
            .Produces<GetHealthResponse>(StatusCodes.Status200OK)
            .Produces<GetHealthResponse>(StatusCodes.Status503ServiceUnavailable);
    }

    internal static async Task<IResult> Handle(IMessageBus bus, CancellationToken ct)
    {
        var response = await bus.InvokeAsync<GetHealthResponse>(new GetHealthRequest(), ct);

        // Container and load-balancer probes only read the status code; Degraded still serves traffic.
        var statusCode =
            response.Status == HealthStatus.Unhealthy
                ? StatusCodes.Status503ServiceUnavailable
                : StatusCodes.Status200OK;

        return Results.Json(response, statusCode: statusCode);
    }
}

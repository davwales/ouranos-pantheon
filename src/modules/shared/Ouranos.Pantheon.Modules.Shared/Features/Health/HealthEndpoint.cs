using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Shared.Features.Health.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Shared.Features.Health;

public static class HealthEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/health", Handle).WithTags("Health").AllowAnonymous();
    }

    internal static async Task<IResult> Handle(IMessageBus bus, CancellationToken ct)
    {
        return Results.Ok(await bus.InvokeAsync<GetHealthResponse>(new GetHealthRequest(), ct));
    }
}

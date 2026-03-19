using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetAllForecasts.Schemas;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetAllForecasts;

public static class GetAllForecastsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/forecasts", Handle)
            .WithTags("Plutus.Forecasts");
    }

    private static async Task<IResult> Handle(
        [AsParameters] GetAllForecastsInput input,
        IScopedDispatcher dispatcher,
        CancellationToken ct = default
    )
    {
        return Results.Ok(await dispatcher.Send(input, ct));
    }
}

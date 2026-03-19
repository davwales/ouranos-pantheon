using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast.Schemas;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast;

public static class GetMarketForecastEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/markets/{marketId}/forecasts", Handle)
            .WithTags("Plutus.Forecasts");
    }

    private static async Task<IResult> Handle(
        [AsParameters] GetMarketForecastInput input,
        IScopedDispatcher dispatcher,
        CancellationToken ct = default
    )
    {
        return Results.Ok(await dispatcher.Send(input, ct));
    }
}

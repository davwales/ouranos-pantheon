using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast.Schemas;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast;

public static class GetMarketForecastEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/markets/{marketId}/forecasts", Handle)
            .WithTags("Plutus.Forecasts");
    }

    internal static async Task<IResult> Handle(
        [AsParameters] GetMarketForecastInput input,
        IMessageBus bus,
        CancellationToken ct = default
    )
    {
        return Results.Ok(await bus.InvokeAsync<PagedResponse<GetMarketForecastResponse>>(input, ct));
    }
}

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetForecastEfficacy.Schemas;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetForecastEfficacy;

public static class GetForecastEfficacyEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/forecasts/efficacy", Handle)
            .WithTags("Plutus.Forecasts");
    }

    internal static async Task<IResult> Handle(
        [AsParameters] GetForecastEfficacyInput input,
        IMessageBus bus,
        CancellationToken ct = default
    )
    {
        return Results.Ok(await bus.InvokeAsync<PagedResponse<GetForecastEfficacyResponse>>(input, ct));
    }
}

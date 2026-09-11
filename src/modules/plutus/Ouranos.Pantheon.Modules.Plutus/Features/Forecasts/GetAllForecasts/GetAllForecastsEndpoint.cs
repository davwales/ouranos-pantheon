using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetAllForecasts.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetAllForecasts;

public static class GetAllForecastsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/forecasts", Handle)
            .CacheOutput(policy =>
                policy
                    .Expire(TimeSpan.FromMinutes(5))
                    .SetVaryByQuery(
                        nameof(GetAllForecastsInput.SortField),
                        nameof(GetAllForecastsInput.SortDirection),
                        nameof(GetAllForecastsInput.Skip),
                        nameof(GetAllForecastsInput.Take),
                        nameof(GetAllForecastsInput.Filter)
                    )
            )
            .WithTags("Plutus.Forecasts");
    }

    internal static async Task<IResult> Handle(
        [AsParameters] GetAllForecastsInput input,
        IMessageBus bus,
        CancellationToken ct = default
    )
    {
        return Results.Ok(await bus.InvokeAsync<PagedResponse<GetAllForecastsResponse>>(input, ct));
    }
}

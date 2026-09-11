using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast;

public static class GetMarketForecastEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/markets/{marketId}/forecasts", Handle)
            .CacheOutput(policy =>
                policy
                    .Expire(TimeSpan.FromMinutes(5))
                    .SetVaryByQuery(
                        nameof(GetMarketForecastInput.SortField),
                        nameof(GetMarketForecastInput.SortDirection),
                        nameof(GetMarketForecastInput.Skip),
                        nameof(GetMarketForecastInput.Take),
                        nameof(GetMarketForecastInput.Filter)
                    )
            )
            .WithTags("Plutus.Forecasts");
    }

    internal static async Task<IResult> Handle(
        [AsParameters] GetMarketForecastInput input,
        IMessageBus bus,
        CancellationToken ct = default
    )
    {
        return Results.Ok(
            await bus.InvokeAsync<PagedResponse<GetMarketForecastResponse>>(input, ct)
        );
    }
}

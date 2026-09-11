using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetForecastEfficacy.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetForecastEfficacy;

public static class GetForecastEfficacyEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/forecasts/efficacy", Handle)
            .CacheOutput(policy =>
                policy
                    .Expire(TimeSpan.FromMinutes(5))
                    .SetVaryByQuery(
                        nameof(GetForecastEfficacyInput.SymbolId),
                        nameof(GetForecastEfficacyInput.MarketId),
                        nameof(GetForecastEfficacyInput.ModelName),
                        nameof(GetForecastEfficacyInput.HorizonDays),
                        nameof(GetForecastEfficacyInput.Since),
                        nameof(GetForecastEfficacyInput.Until),
                        nameof(GetForecastEfficacyInput.Skip),
                        nameof(GetForecastEfficacyInput.Take)
                    )
            )
            .WithTags("Plutus.Forecasts");
    }

    internal static async Task<IResult> Handle(
        [AsParameters] GetForecastEfficacyInput input,
        IMessageBus bus,
        CancellationToken ct = default
    )
    {
        return Results.Ok(
            await bus.InvokeAsync<PagedResponse<GetForecastEfficacyResponse>>(input, ct)
        );
    }
}

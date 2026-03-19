using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetAllForecasts.Schemas;

public sealed record GetAllForecastsInput(
    string? SortField = null,
    string? SortDirection = null,
    int Skip = 0,
    int Take = 10,
    string[]? Filter = null
) : IQuery<PagedResponse<GetAllForecastsResponse>>;

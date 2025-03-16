using Ouranos.Pantheon.Core.API.Queries;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Application.Queries.Common.GetAllEntities;
using Ouranos.Pantheon.Core.Application.Queries.Common.GetEntity;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Application.Queries.Markets.GetMarketForecast;
using Ouranos.Pantheon.Service.Plutus.Application.Queries.Markets.GetMarketTrades;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;

namespace Ouranos.Pantheon.Service.Plutus.API.Queries;

[ExtendObjectType<Query>]
public sealed class MarketQueries
{
    /// <summary>
    ///     Gets all markets.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="cancellationToken"><see cref="CancellationToken" />.</param>
    /// <returns>List of all markets.</returns>
    [UsePaging(IncludeTotalCount = true)]
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<Market>> GetAllMarkets(
        [Service] IDispatcher dispatcher,
        CancellationToken cancellationToken = default
    )
    {
        var wrapper = await dispatcher.Send(new GetAllEntitiesInput<Market>(), cancellationToken);
        return wrapper.Value;
    }

    /// <summary>
    ///     Retrieves a market by it's identifier.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="marketId">Id of the market to get.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken" />.</param>
    /// <returns>The market matching the given query.</returns>
    public async Task<Market> GetMarket(
        [Service] IDispatcher dispatcher,
        Id<Market> marketId,
        CancellationToken cancellationToken = default
    )
    {
        return await dispatcher.Send(new GetEntityInput<Market>(marketId), cancellationToken);
    }

    /// <summary>
    ///     Retrieves information about the symbols in a market.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="input">Query used to filter market trades.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken" />.</param>
    /// <returns>Trade statistics for the symbols in a market.</returns>
    [UsePaging(IncludeTotalCount = true)]
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<GetMarketTradesResponse>> GetMarketTrades(
        [Service] IDispatcher dispatcher,
        GetMarketTradesInput input,
        CancellationToken cancellationToken = default
    )
    {
        var wrapper = await dispatcher.Send(input, cancellationToken);
        return wrapper.Value;
    }

    /// <summary>
    ///     Gets all forecasts.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="input">Input variables required to retrieve forecasts.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken" />.</param>
    /// <returns>List of all forecasts.</returns>
    [UsePaging(IncludeTotalCount = true)]
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<GetMarketForecastResponse>> GetMarketForecast(
        [Service] IDispatcher dispatcher,
        GetMarketForecastInput input,
        CancellationToken cancellationToken = default
    )
    {
        var wrapper = await dispatcher.Send(input, cancellationToken);
        return wrapper.Value;
    }
}
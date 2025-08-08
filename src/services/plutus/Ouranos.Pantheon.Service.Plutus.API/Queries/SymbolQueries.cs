using Ouranos.Pantheon.Core.API.Queries;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Application.Queries.Common.GetAllEntities;
using Ouranos.Pantheon.Core.Application.Queries.Common.GetEntity;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Application.Queries.Symbols.GetDailySymbolSummary;
using Ouranos.Pantheon.Service.Plutus.Application.Queries.Symbols.GetSymbolTrades;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

namespace Ouranos.Pantheon.Service.Plutus.API.Queries;

[ExtendObjectType<Query>]
public sealed class SymbolQueries
{
    /// <summary>
    ///     Gets all symbols.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>Queryable list of all symbols.</returns>
    [UsePaging(IncludeTotalCount = true)]
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<Symbol>> GetAllSymbols(
        [Service] IScopedDispatcher dispatcher,
        CancellationToken cancellationToken = default
    )
    {
        var wrapper = await dispatcher.Send(new GetAllEntitiesInput<Symbol>(), cancellationToken);
        return wrapper.Value;
    }

    /// <summary>
    ///     Gets a symbol by its identifier.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="symbolId">Id of the symbol to get.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken" />.</param>
    /// <returns>The symbol matching the given query.</returns>
    public async Task<Symbol> GetSymbol(
        [Service] IScopedDispatcher dispatcher,
        Id<Symbol> symbolId,
        CancellationToken cancellationToken = default
    )
    {
        return await dispatcher.Send(new GetEntityInput<Symbol>(symbolId), cancellationToken);
    }

    /// <summary>
    ///     Retrieves information about the trades for a given symbol.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="input">Query used to filter symbol trades.</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>Trade statistics for a symbol.</returns>
    public async Task<GetSymbolTradesResponse> GetSymbolTrades(
        [Service] IScopedDispatcher dispatcher,
        GetSymbolTradesInput input,
        CancellationToken cancellationToken = default
    )
    {
        return await dispatcher.Send(input, cancellationToken);
    }

    /// <summary>
    ///     Gets the daily summary of trades for a symbol.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="input">Inputs required to retrieve the summary.</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>Summary of trades for the given symbol for today.</returns>
    public async Task<GetDailySymbolSummaryResponse> GetDailySymbolSummary(
        [Service] IScopedDispatcher dispatcher,
        GetDailySymbolSummaryInput input,
        CancellationToken cancellationToken = default
    )
    {
        return await dispatcher.Send(input, cancellationToken);
    }
}
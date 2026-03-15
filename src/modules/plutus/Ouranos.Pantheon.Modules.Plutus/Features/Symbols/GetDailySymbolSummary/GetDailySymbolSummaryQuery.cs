using HotChocolate;
using HotChocolate.Types;
using Ouranos.Pantheon.Core.API.Queries;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetDailySymbolSummary.Schemas;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetDailySymbolSummary;

[ExtendObjectType<Query>]
public sealed class GetDailySymbolSummaryQuery
{
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

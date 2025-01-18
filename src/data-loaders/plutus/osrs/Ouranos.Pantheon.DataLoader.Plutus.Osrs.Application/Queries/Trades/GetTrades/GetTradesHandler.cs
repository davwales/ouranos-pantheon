using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.DataLoader.Plutus.Osrs.Application.Interfaces.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Osrs.Application.Queries.Trades.GetTrades;

public sealed class GetTradesHandler : QueryHandler<GetTradesInput, WrapperResponse<List<GetTradesResponse>>>
{
    private readonly IGetTrades _getTrades;
    private readonly ILogger<GetTradesHandler> _logger;

    public GetTradesHandler(
        ILogger<GetTradesHandler> logger,
        IGetTrades getTrades
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(getTrades);

        _logger = logger;
        _getTrades = getTrades;
    }

    protected override async Task<WrapperResponse<List<GetTradesResponse>>> Handle(
        GetTradesInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get trades query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var trades = await _getTrades.GetTradesAsync(cancellationToken);
        var response = new WrapperResponse<List<GetTradesResponse>>(trades);

        _logger.LogDebug("Successfully retrieves '{tradeCount}' trades.", trades.Count);
        return response;
    }
}
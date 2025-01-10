using MediatR;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.DataLoader.Plutus.Osrs.Application.Interfaces.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Osrs.Application.Queries.Trades.GetTrades;

public sealed class GetTradesHandler : IRequestHandler<GetTradesInput, List<GetTradesResponse>>
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

    public async Task<List<GetTradesResponse>> Handle(
        GetTradesInput request,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get trades request '{@request}'.", request);
        cancellationToken.ThrowIfCancellationRequested();

        var trades = await _getTrades.GetTradesAsync(cancellationToken);

        _logger.LogDebug("Successfully retrieves '{tradeCount}' trades.", trades.Count);
        return trades;
    }
}
using MassTransit;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Mediator;
using Ouranos.Pantheon.DataLoader.Plutus.Osrs.Application.Interfaces.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Osrs.Application.Queries.Trades.GetTrades;

public sealed class GetTradesHandler : IQueryHandler<GetTradesInput, WrapperResponse<List<GetTradesResponse>>>
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

    public async Task Consume(ConsumeContext<GetTradesInput> context)
    {
        _logger.LogTrace("Attempting to handle get trades query '{@query}'.", context.Message);
        context.CancellationToken.ThrowIfCancellationRequested();

        var trades = await _getTrades.GetTradesAsync(context.CancellationToken);

        _logger.LogDebug("Successfully retrieves '{tradeCount}' trades.", trades.Count);
        await context.RespondAsync(new WrapperResponse<List<GetTradesResponse>>(trades));
    }
}
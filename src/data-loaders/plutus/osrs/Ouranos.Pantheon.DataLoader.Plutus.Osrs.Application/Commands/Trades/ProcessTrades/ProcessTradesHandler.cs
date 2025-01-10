using MediatR;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.DataLoader.Plutus.Domain.Trades;
using Ouranos.Pantheon.DataLoader.Plutus.Osrs.Application.Interfaces.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Osrs.Application.Commands.Trades.ProcessTrades;

public sealed class ProcessTradesHandler : IRequestHandler<ProcessTradesInput>
{
    private readonly ILogger<ProcessTradesHandler> _logger;
    private readonly IQueueTradeMessage _queueTradeMessage;

    public ProcessTradesHandler(
        ILogger<ProcessTradesHandler> logger,
        IQueueTradeMessage queueTradeMessage
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(queueTradeMessage);

        _logger = logger;
        _queueTradeMessage = queueTradeMessage;
    }

    public async Task Handle(
        ProcessTradesInput request,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle process trades request '{@request}'", request);
        cancellationToken.ThrowIfCancellationRequested();

        var processedCount = 0;
        foreach (var trade in request.Trades)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var message = new TradeMessage(
                Producer.Osrs,
                trade.SymbolCode,
                trade.SymbolSubCode,
                trade.SymbolName,
                trade.GetTradesAdditionalFieldsResponse.Limit,
                trade.Price,
                trade.Volume,
                trade.Timestamp,
                new Dictionary<string, object?>
                {
                    { "highalch", trade.GetTradesAdditionalFieldsResponse.HighAlch },
                    { "lowalch", trade.GetTradesAdditionalFieldsResponse.LowAlch },
                    { "limit", trade.GetTradesAdditionalFieldsResponse.Limit }
                }
            );

            await _queueTradeMessage.QueueMessage(message, cancellationToken);
            processedCount++;
        }

        _logger.LogInformation("Successfully processed '{processedCount}' trades.", processedCount);
    }
}
using MediatR;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.DataLoader.Plutus.Application.Interfaces.Trades;
using Ouranos.Pantheon.DataLoader.Plutus.Domain;
using Ouranos.Pantheon.DataLoader.Plutus.Domain.Trades;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

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
                trade.SymbolSubcode,
                trade.SymbolName,
                trade.Price,
                trade.Volume,
                trade.Timestamp,
                new AdditionalFields(
                    trade.GetTradesAdditionalFieldsResponse.Limit,
                    trade.GetTradesAdditionalFieldsResponse.HighAlch,
                    trade.GetTradesAdditionalFieldsResponse.LowAlch
                )
            );

            await _queueTradeMessage.QueueMessage(message, cancellationToken);
            processedCount++;
        }

        _logger.LogInformation("Successfully processed '{processedCount}' trades.", processedCount);
    }
}
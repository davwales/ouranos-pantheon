using MediatR;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.DataLoader.Plutus.Application.Interfaces.Trades;
using Ouranos.Pantheon.DataLoader.Plutus.Domain;
using Ouranos.Pantheon.DataLoader.Plutus.Domain.Trades;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Application.Interfaces.Items;

namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Application.Commands.Trades.ProcessTrade;

public sealed class ProcessTradeHandler : IRequestHandler<ProcessTradeInput>
{
    private readonly IGetItems _getItems;
    private readonly ILogger<ProcessTradeHandler> _logger;
    private readonly IQueueTradeMessage _queueTradeMessage;

    public ProcessTradeHandler(
        ILogger<ProcessTradeHandler> logger,
        IGetItems getItems,
        IQueueTradeMessage queueTradeMessage
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(getItems);
        ArgumentNullException.ThrowIfNull(queueTradeMessage);

        _logger = logger;
        _getItems = getItems;
        _queueTradeMessage = queueTradeMessage;
    }

    public async Task Handle(
        ProcessTradeInput request,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle process message request '{@request}'.", request);
        cancellationToken.ThrowIfCancellationRequested();

        if (request.Sales.Count == 0)
        {
            _logger.LogDebug("There are no sales to process.");
            return;
        }

        var itemDtos = await _getItems.GetItemsAsync(cancellationToken);

        var processedCount = 0;
        foreach (var sale in request.Sales)
        {
            var item = itemDtos.FirstOrDefault(i =>
                i.SymbolCode == request.ItemCode && i.IsHighQuality == sale.IsHighQuality);
            if (item is null)
            {
                _logger.LogWarning("Trade item '{itemCode}' '{isHighQuality}' is missing.", request.ItemCode,
                    sale.IsHighQuality);
                continue;
            }

            const string hqCode = "hq";
            const string lqCode = "lq";

            var tradeMessage = new TradeMessage(
                Producer.Ffxiv,
                request.ItemCode,
                sale.IsHighQuality ? hqCode : lqCode,
                item.SymbolName,
                null,
                sale.Price,
                sale.Volume,
                sale.Timestamp,
                item.AdditionalFields
            );

            await _queueTradeMessage.QueueMessage(tradeMessage, cancellationToken);
            processedCount++;
        }

        _logger.LogInformation("Successfully processed '{processedCount}' trades.", processedCount);
    }
}
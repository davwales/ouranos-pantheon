using MediatR;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.DataLoader.Plutus.Application.Interfaces.Trades;
using Ouranos.Pantheon.DataLoader.Plutus.Domain;
using Ouranos.Pantheon.DataLoader.Plutus.Domain.Trades;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Application.Interfaces.Items;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Application.Interfaces.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Application.Commands.Messages.ProcessMessage;

public sealed class ProcessMessageHandler : IRequestHandler<ProcessMessageInput>
{
    private readonly IGetItems _getItems;
    private readonly ILogger<ProcessMessageHandler> _logger;
    private readonly IParseTrades _parseTrades;
    private readonly IQueueTradeMessage _queueTradeMessage;

    public ProcessMessageHandler(
        ILogger<ProcessMessageHandler> logger,
        IParseTrades parseTrades,
        IGetItems getItems,
        IQueueTradeMessage queueTradeMessage
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(parseTrades);
        ArgumentNullException.ThrowIfNull(getItems);
        ArgumentNullException.ThrowIfNull(queueTradeMessage);

        _logger = logger;
        _parseTrades = parseTrades;
        _getItems = getItems;
        _queueTradeMessage = queueTradeMessage;
    }

    public async Task Handle(
        ProcessMessageInput request,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle process message request '{@request}'.", request);
        cancellationToken.ThrowIfCancellationRequested();

        var tradeDtos = await _parseTrades.ParseTradeMessage(
            request.Message,
            cancellationToken
        );

        if (tradeDtos.Count == 0)
        {
            _logger.LogDebug("There are no trades to process.");
            return;
        }

        var itemDtos = await _getItems.GetItemsAsync(cancellationToken);

        var processedCount = 0;
        foreach (var tradeDto in tradeDtos)
        {
            var item = itemDtos.FirstOrDefault(i =>
                i.SymbolCode == tradeDto.SymbolCode && i.IsHighQuality == tradeDto.IsHighQuality);
            if (item is null)
            {
                _logger.LogWarning("Trade item '{itemCode}' '{isHighQuality}' is missing.", tradeDto.SymbolCode,
                    tradeDto.IsHighQuality);
                continue;
            }

            const string hqCode = "hq";
            const string lqCode = "lq";

            var tradeMessage = new TradeMessage(
                Producer.Ffxiv,
                tradeDto.SymbolCode,
                tradeDto.IsHighQuality ? hqCode : lqCode,
                item.SymbolName,
                null,
                tradeDto.Price,
                tradeDto.Volume,
                tradeDto.Timestamp,
                item.AdditionalFields
            );

            await _queueTradeMessage.QueueMessage(tradeMessage, cancellationToken);
            processedCount++;
        }

        _logger.LogInformation("Successfully processed '{processedCount}' trades.", processedCount);
    }
}
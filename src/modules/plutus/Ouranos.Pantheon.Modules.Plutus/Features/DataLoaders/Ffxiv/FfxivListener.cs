using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.Messages;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.XivApi;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Shared;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Listeners;
using Ouranos.Pantheon.Modules.Shared.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv;

public sealed class FfxivListener : IListener<SaleMessage>
{
    private readonly ILogger<FfxivListener> _logger;
    private readonly IGetItems _getItems;
    private readonly IQueueTradeMessages _queueTradeMessages;

    public FfxivListener(
        ILogger<FfxivListener> logger,
        IGetItems getItems,
        IQueueTradeMessages queueTradeMessages
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(getItems);
        Guard.Against.Null(queueTradeMessages);

        _logger = logger;
        _getItems = getItems;
        _queueTradeMessages = queueTradeMessages;
    }

    public async Task HandleMessageAsync(
        SaleMessage message,
        IWebSocketClient _,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Handling message '{message}'.", message);
        cancellationToken.ThrowIfCancellationRequested();

        var sales = message
            .Sales.Select(s => new ProcessTradeSaleInput(
                s.Hq,
                s.PricePerUnit,
                s.Quantity,
                DateTimeOffset.FromUnixTimeSeconds(s.Timestamp)
            ))
            .ToList();

        if (sales.Count == 0)
        {
            _logger.LogDebug("There are no sales to process.");
            return;
        }

        var itemCode = message.Item.ToString();
        var itemDtos = await _getItems.GetItemsAsync(cancellationToken);

        const string hqCode = "hq";
        var hqItem = itemDtos.FirstOrDefault(i => i.SymbolCode == itemCode && i.IsHighQuality);

        const string nqCode = "nq";
        var nqItem = itemDtos.FirstOrDefault(i => i.SymbolCode == itemCode && !i.IsHighQuality);

        var messages = sales
            .Select(sale =>
            {
                var item = sale.IsHighQuality ? hqItem : nqItem;
                if (item is not null)
                {
                    return new TradeMessage(
                        Producer.Ffxiv,
                        itemCode,
                        sale.IsHighQuality ? hqCode : nqCode,
                        item.SymbolName,
                        sale.Price,
                        sale.Volume,
                        sale.Timestamp,
                        item.AdditionalFields
                    );
                }

                _logger.LogWarning(
                    "Trade item '{itemCode}' '{isHighQuality}' is missing.",
                    itemCode,
                    sale.IsHighQuality
                );
                return null;
            })
            .Where(x => x is not null)
            .OfType<TradeMessage>()
            .ToList();

        await _queueTradeMessages.QueueMessages(messages, cancellationToken);
        _logger.LogDebug("Successfully handled message.");
    }
}

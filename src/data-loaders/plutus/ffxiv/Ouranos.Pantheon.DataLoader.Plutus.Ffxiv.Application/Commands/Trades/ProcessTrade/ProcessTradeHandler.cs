using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.DataLoader.Plutus.Application.Interfaces.Trades;
using Ouranos.Pantheon.DataLoader.Plutus.Domain;
using Ouranos.Pantheon.DataLoader.Plutus.Domain.Trades;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Application.Interfaces.Items;

namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Application.Commands.Trades.ProcessTrade;

public sealed class ProcessTradeHandler : CommandHandler<ProcessTradeInput>
{
    private readonly IGetItems _getItems;
    private readonly ILogger<ProcessTradeHandler> _logger;
    private readonly IQueueTradeMessages _queueTradeMessages;

    public ProcessTradeHandler(
        ILogger<ProcessTradeHandler> logger,
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

    protected override async Task Handle(
        ProcessTradeInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle process message command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        if (command.Sales.Count == 0)
        {
            _logger.LogDebug("There are no sales to process.");
            return;
        }

        var itemDtos = await _getItems.GetItemsAsync(cancellationToken);

        const string hqCode = "hq";
        var hqItem = itemDtos.FirstOrDefault(i => i.SymbolCode == command.ItemCode && i.IsHighQuality);

        const string lqCode = "lq";
        var lqItem = itemDtos.FirstOrDefault(i => i.SymbolCode == command.ItemCode && !i.IsHighQuality);

        var messages = command.Sales
            .Select(sale =>
            {
                var item = sale.IsHighQuality ? hqItem : lqItem;
                if (item is not null)
                {
                    return new TradeMessage(
                        Producer.Ffxiv,
                        command.ItemCode,
                        sale.IsHighQuality ? hqCode : lqCode,
                        item.SymbolName,
                        sale.Price,
                        sale.Volume,
                        sale.Timestamp,
                        item.AdditionalFields
                    );
                }

                _logger.LogWarning("Trade item '{itemCode}' '{isHighQuality}' is missing.", command.ItemCode,
                    sale.IsHighQuality);
                return null;
            })
            .Where(x => x is not null)
            .OfType<TradeMessage>()
            .ToList();

        await _queueTradeMessages.QueueMessages(messages, cancellationToken);

        _logger.LogInformation("Successfully processed '{messageCount}' trades.", messages.Count);
    }
}
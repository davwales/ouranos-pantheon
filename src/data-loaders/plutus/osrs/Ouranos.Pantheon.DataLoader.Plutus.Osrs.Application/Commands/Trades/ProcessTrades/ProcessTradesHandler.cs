using MassTransit;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Interfaces.Mediator;
using Ouranos.Pantheon.DataLoader.Plutus.Application.Interfaces.Trades;
using Ouranos.Pantheon.DataLoader.Plutus.Domain;
using Ouranos.Pantheon.DataLoader.Plutus.Domain.Trades;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Osrs.Application.Commands.Trades.ProcessTrades;

public sealed class ProcessTradesHandler : ICommandHandler<ProcessTradesInput>
{
    private readonly ILogger<ProcessTradesHandler> _logger;
    private readonly IQueueTradeMessages _queueTradeMessages;

    public ProcessTradesHandler(
        ILogger<ProcessTradesHandler> logger,
        IQueueTradeMessages queueTradeMessages
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(queueTradeMessages);

        _logger = logger;
        _queueTradeMessages = queueTradeMessages;
    }

    public async Task Consume(ConsumeContext<ProcessTradesInput> context)
    {
        _logger.LogTrace("Attempting to handle process trades command '{@command}'", context.Message);
        context.CancellationToken.ThrowIfCancellationRequested();

        var messages = context.Message.Trades.Select(trade => new TradeMessage(
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
        )).ToList();

        await _queueTradeMessages.QueueMessages(messages, context.CancellationToken);
        _logger.LogInformation("Successfully processed '{messageCount}' trades.", messages.Count);
    }
}
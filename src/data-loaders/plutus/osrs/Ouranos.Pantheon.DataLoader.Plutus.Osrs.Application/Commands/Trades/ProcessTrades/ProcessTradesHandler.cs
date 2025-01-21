using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.DataLoader.Plutus.Application.Interfaces.Trades;
using Ouranos.Pantheon.DataLoader.Plutus.Domain;
using Ouranos.Pantheon.DataLoader.Plutus.Domain.Trades;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Osrs.Application.Commands.Trades.ProcessTrades;

public sealed class ProcessTradesHandler : CommandHandler<ProcessTradesInput>
{
    private readonly ILogger<ProcessTradesHandler> _logger;
    private readonly IQueueTradeMessages _queueTradeMessages;

    public ProcessTradesHandler(
        ILogger<ProcessTradesHandler> logger,
        IQueueTradeMessages queueTradeMessages
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(queueTradeMessages);

        _logger = logger;
        _queueTradeMessages = queueTradeMessages;
    }

    protected override async Task Handle(
        ProcessTradesInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle process trades command '{@command}'", command);
        cancellationToken.ThrowIfCancellationRequested();

        var messages = command.Trades.Select(trade => new TradeMessage(
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

        await _queueTradeMessages.QueueMessages(messages, cancellationToken);
        _logger.LogInformation("Successfully processed '{messageCount}' trades.", messages.Count);
    }
}
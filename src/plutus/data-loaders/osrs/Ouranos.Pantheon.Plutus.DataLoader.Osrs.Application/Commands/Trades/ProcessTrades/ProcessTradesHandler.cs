using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Plutus.DataLoader.Application.Interfaces.Trades;
using Ouranos.Pantheon.Plutus.DataLoader.Domain;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;
using TradeMessage = Ouranos.Pantheon.Plutus.DataLoader.Domain.Trades.TradeMessage;

namespace Ouranos.Pantheon.Plutus.DataLoader.Osrs.Application.Commands.Trades.ProcessTrades;

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

    public override async Task Handle(
        ProcessTradesInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle process trades command '{@command}'", command);
        cancellationToken.ThrowIfCancellationRequested();

        if (command.Trades.Count == 0)
        {
            _logger.LogDebug("There are no trades to process.");
            return;
        }

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
            )
        ).ToList();

        await _queueTradeMessages.QueueMessages(messages, cancellationToken);
        _logger.LogInformation("Successfully processed '{messageCount}' trades.", messages.Count);
    }
}
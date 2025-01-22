using Ardalis.GuardClauses;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.DataLoader.Plutus.Application.Interfaces.Trades;
using Ouranos.Pantheon.DataLoader.Plutus.TalosMigration.Handlers.GetTrades;
using Ouranos.Pantheon.DataLoader.Plutus.TalosMigration.Handlers.ProcessTrade;
using Ouranos.Pantheon.DataLoader.Plutus.TalosMigration.Models;

namespace Ouranos.Pantheon.DataLoader.Plutus.TalosMigration;

public sealed class TalosProducer : IHostedService
{
    private readonly IDispatcher _dispatcher;
    private readonly ILogger<TalosProducer> _logger;
    private readonly IQueueTradeMessages _queueTradeMessages;

    public TalosProducer(
        IDispatcher dispatcher,
        ILogger<TalosProducer> logger,
        IQueueTradeMessages queueTradeMessages
    )
    {
        Guard.Against.Null(dispatcher);
        Guard.Against.Null(logger);
        Guard.Against.Null(queueTradeMessages);

        _logger = logger;
        _dispatcher = dispatcher;
        _queueTradeMessages = queueTradeMessages;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var getTalosTradesInput = new GetTradesInput();
        var tradeResponse = await _dispatcher.Send(getTalosTradesInput, cancellationToken);

        var batch = 0;
        var tradesProcessed = 0;
        while (await tradeResponse.Cursor.MoveNextAsync(cancellationToken))
        {
            var start = DateTimeOffset.UtcNow;
            var tasks = tradeResponse.Cursor.Current.Select(t => ProcessTrade(t, cancellationToken));
            var responses = await Task.WhenAll(tasks);
            var duration = DateTimeOffset.UtcNow.Subtract(start);

            batch++;
            tradesProcessed += responses.Count(r => r.WasProcessed);
            _logger.LogInformation(
                "Processed '{tradeCount}' trades in '{seconds}' seconds for batch '{batchCount}'.",
                tradesProcessed, duration.Seconds, batch
            );
        }

        _logger.LogInformation("Completed Talos trade migration.");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }

    private async Task<ProcessTradeResponse> ProcessTrade(TalosTrade? trade, CancellationToken cancellationToken)
    {
        var convertTradeInput = new ProcessTradeInput(trade);
        return await _dispatcher.Send(convertTradeInput, cancellationToken);
    }
}
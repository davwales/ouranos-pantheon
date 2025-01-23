using Ardalis.GuardClauses;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Infra.Mongo;
using Ouranos.Pantheon.DataLoader.Plutus.Application.Interfaces.Trades;
using Ouranos.Pantheon.DataLoader.Plutus.TalosMigration.Actions.ConvertTrade;
using Ouranos.Pantheon.DataLoader.Plutus.TalosMigration.Actions.GetTrades;
using Ouranos.Pantheon.DataLoader.Plutus.TalosMigration.Models;

namespace Ouranos.Pantheon.DataLoader.Plutus.TalosMigration;

public sealed class TalosProducer : IHostedService
{
    private readonly IConvertTradeAction _convertTrade;
    private readonly IGetTradesAction _getTrades;
    private readonly ILogger<TalosProducer> _logger;
    private readonly IQueueTradeMessages _queueTradeMessages;
    private readonly IMongoRepository<TradeMigration> _tradeMigrationRepository;

    public TalosProducer(
        ILogger<TalosProducer> logger,
        IGetTradesAction getTrades,
        IConvertTradeAction convertTrade,
        IQueueTradeMessages queueTradeMessages,
        IMongoRepository<TradeMigration> tradeMigrationRepository
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(getTrades);
        Guard.Against.Null(convertTrade);
        Guard.Against.Null(queueTradeMessages);
        Guard.Against.Null(tradeMigrationRepository);

        _logger = logger;
        _getTrades = getTrades;
        _convertTrade = convertTrade;
        _queueTradeMessages = queueTradeMessages;
        _tradeMigrationRepository = tradeMigrationRepository;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogTrace("Starting trade producer...");
        var tradeCursor = await _getTrades.GetTradesAsync(cancellationToken);

        long numProcessed = 0;
        var start = DateTimeOffset.UtcNow;

        while (await tradeCursor.MoveNextAsync(cancellationToken))
        {
            var migrations = tradeCursor.Current.Select(t => new TradeMigration(t.Id)).ToList();
            var convertedTrades = tradeCursor.Current.Select(_convertTrade.ConvertTrade).ToList();
            var messages = convertedTrades.Where(t => t is not null).Select(t => t!).ToList();

            await _queueTradeMessages.QueueMessages(messages, cancellationToken);
            await _tradeMigrationRepository.GetCollection().InsertManyAsync(migrations, null, cancellationToken);

            numProcessed += migrations.Count;
            var duration = DateTimeOffset.UtcNow.Subtract(start);
            _logger.LogInformation(
                "Processed '{count}' trades after a total of '{seconds}' seconds. Throughput: '{throughput}' trades/s.",
                numProcessed,
                duration.TotalSeconds,
                duration.TotalSeconds > 0 ? numProcessed / duration.TotalSeconds : 0
            );
        }

        _logger.LogInformation("Completed producing trades.");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}
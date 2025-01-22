using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.DataLoader.Plutus.Application.Interfaces.Trades;
using Ouranos.Pantheon.DataLoader.Plutus.Domain;
using Ouranos.Pantheon.DataLoader.Plutus.Domain.Trades;
using Ouranos.Pantheon.DataLoader.Plutus.TalosMigration.Models;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.TalosMigration.Handlers.ProcessTrade;

public sealed class ProcessTradeHandler : CommandHandler<ProcessTradeInput, ProcessTradeResponse>
{
    private readonly ILogger<ProcessTradeHandler> _logger;
    private readonly Dictionary<ObjectId, Producer> _producerMap;
    private readonly IQueueTradeMessages _queueTradeMessages;
    private readonly ICrudRepository<TradeMigration> _tradeMigrationRepository;

    public ProcessTradeHandler(
        ILogger<ProcessTradeHandler> logger,
        ICrudRepository<TradeMigration> tradeMigrationRepository,
        IConfiguration configuration,
        IQueueTradeMessages queueTradeMessages
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(tradeMigrationRepository);
        Guard.Against.Null(configuration);
        Guard.Against.Null(queueTradeMessages);

        _logger = logger;
        _tradeMigrationRepository = tradeMigrationRepository;
        _queueTradeMessages = queueTradeMessages;
        _producerMap = configuration
                           .GetSection("Ouranos:Markets")
                           .Get<Dictionary<string, Producer>>()
                           ?.ToDictionary(x => new ObjectId(x.Key), x => x.Value)
                       ?? throw new InvalidOperationException("Cannot find market map in configuration.");
    }

    protected override async Task<ProcessTradeResponse> Handle(
        ProcessTradeInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle process trade command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        if (command.Trade?.MetaData?.Symbol is null || string.IsNullOrWhiteSpace(command.Trade.MetaData.Symbol.Code))
        {
            _logger.LogError("Skipping invalid trade.");
            return new ProcessTradeResponse(false);
        }

        var tradeMigrationId = new Id<TradeMigration>(command.Trade.Id.ToString());
        if (await IsDuplicate(tradeMigrationId, cancellationToken))
        {
            _logger.LogDebug("Skipping duplicate trade '{tradeId}'.", command.Trade.Id);
            return new ProcessTradeResponse(false);
        }

        if (!_producerMap.TryGetValue(command.Trade.MetaData.Symbol.MarketId, out var producer))
        {
            _logger.LogError("Failed to find valid producer for market '{marketId}'.",
                command.Trade.MetaData.Symbol.MarketId);
            return new ProcessTradeResponse(false);
        }

        var message = new TradeMessage(
            producer,
            command.Trade.MetaData.Symbol.Code,
            command.Trade.MetaData.Symbol.Subcode,
            command.Trade.MetaData.Symbol.Name,
            command.Trade.Price,
            command.Trade.Volume,
            command.Trade.Date,
            new AdditionalFields(
                command.Trade.MetaData.Symbol.AdditionalFields?.Limit,
                command.Trade.MetaData.Symbol.AdditionalFields?.HighAlch,
                command.Trade.MetaData.Symbol.AdditionalFields?.LowAlch
            )
        );

        await _queueTradeMessages.QueueMessages([message], cancellationToken);
        await _tradeMigrationRepository.Create(new TradeMigration(tradeMigrationId), cancellationToken);

        _logger.LogDebug("Successfully handled process trade request.");
        return new ProcessTradeResponse(true);
    }

    private async Task<bool> IsDuplicate(Id<TradeMigration> tradeId, CancellationToken cancellationToken)
    {
        var existingMigration = await _tradeMigrationRepository.FirstOrDefault(
            m => m.Id == tradeId,
            cancellationToken
        );

        return existingMigration is not null;
    }
}
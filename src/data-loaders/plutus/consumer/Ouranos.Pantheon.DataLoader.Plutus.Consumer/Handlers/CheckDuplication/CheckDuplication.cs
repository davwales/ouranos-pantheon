using Ardalis.GuardClauses;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Consumer.Handlers.CheckDuplication;

public sealed class CheckDuplication : ICheckDuplication
{
    private readonly ILogger<CheckDuplication> _logger;
    private readonly ICrudRepository<Trade> _tradesRepository;

    public CheckDuplication(
        ILogger<CheckDuplication> logger,
        ICrudRepository<Trade> tradesRepository
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(tradesRepository);

        _logger = logger;
        _tradesRepository = tradesRepository;
    }

    public async Task<bool> CheckDuplicationAsync(
        Guid messageId,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to check if message '{messageId}' is a duplicate.", messageId);
        cancellationToken.ThrowIfCancellationRequested();

        var trade = await _tradesRepository.FirstOrDefault(
            t => t.Metadata.MessageId == messageId,
            cancellationToken
        );

        var isDuplicate = trade is not null;

        _logger.LogDebug("Successfully checked if message was a duplicate.");
        return isDuplicate;
    }
}
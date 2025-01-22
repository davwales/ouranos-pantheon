using Ardalis.GuardClauses;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Consumer.Handlers.CheckDuplication;

public sealed class CheckDuplicationHandler : QueryHandler<CheckDuplicationInput, CheckDuplicationResponse>
{
    private readonly ILogger<CheckDuplicationHandler> _logger;
    private readonly ICrudRepository<Trade> _tradesRepository;

    public CheckDuplicationHandler(
        ILogger<CheckDuplicationHandler> logger,
        ICrudRepository<Trade> tradesRepository
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(tradesRepository);

        _logger = logger;
        _tradesRepository = tradesRepository;
    }

    protected override async Task<CheckDuplicationResponse> Handle(
        CheckDuplicationInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle check duplication query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var trade = await _tradesRepository.FirstOrDefault(
            t => t.Metadata.MessageId == query.MessageId,
            cancellationToken
        );

        var isDuplicate = trade is not null;
        var response = new CheckDuplicationResponse(isDuplicate);

        _logger.LogDebug("Successfully handled check duplicate query.");
        return response;
    }
}
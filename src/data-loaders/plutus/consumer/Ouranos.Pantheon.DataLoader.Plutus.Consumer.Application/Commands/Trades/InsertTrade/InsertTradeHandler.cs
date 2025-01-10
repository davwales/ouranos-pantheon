using MediatR;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Consumer.Application.Commands.Trades.InsertTrade;

public sealed class InsertTradeHandler : IRequestHandler<InsertTradeInput, Trade>
{
    private readonly ICreateDatabaseId<Trade> _createDatabaseId;
    private readonly ILogger<InsertTradeHandler> _logger;
    private readonly ICrudRepository<Trade> _tradeRepository;

    public InsertTradeHandler(
        ILogger<InsertTradeHandler> logger,
        ICreateDatabaseId<Trade> createDatabaseId,
        ICrudRepository<Trade> tradeRepository
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(createDatabaseId);
        ArgumentNullException.ThrowIfNull(tradeRepository);

        _logger = logger;
        _createDatabaseId = createDatabaseId;
        _tradeRepository = tradeRepository;
    }

    public async Task<Trade> Handle(
        InsertTradeInput request,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle process insert trade request '{@request}'.", request);
        cancellationToken.ThrowIfCancellationRequested();

        var trade = new Trade(
            _createDatabaseId.CreateId(),
            request.Price,
            request.Volume,
            new TradeMetadata(
                new TradeSymbolMetadata(
                    request.SymbolId,
                    request.MarketId,
                    request.SymbolName,
                    request.SymbolCode,
                    request.SymbolSubCode,
                    new AdditionalFields(request.Limit)
                )
            ),
            request.Timestamp
        );

        await _tradeRepository.Create(trade, cancellationToken);

        _logger.LogDebug("Successfully handled insert trade request.");
        return trade;
    }
}
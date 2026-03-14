using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;

namespace Ouranos.Pantheon.Plutus.Service.Application.Commands.Markets.CreateMarket;

public sealed class CreateMarketHandler : CommandHandler<CreateMarketInput, IdResponse<Market>>
{
    private readonly ILogger<CreateMarketHandler> _logger;
    private readonly IPlutusUnitOfWork _unitOfWork;

    public CreateMarketHandler(
        ILogger<CreateMarketHandler> logger,
        IPlutusUnitOfWork unitOfWork
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(unitOfWork);

        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public override async Task<IdResponse<Market>> Handle(
        CreateMarketInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogDebug("Attempting to handle create market command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var marketId = _unitOfWork.Markets.CreateId();
        var market = Market.Create(marketId, command.Name, command.Taxes);
        await _unitOfWork.Markets.Create(market, cancellationToken);
        await _unitOfWork.SaveChanges(cancellationToken);
        var response = new IdResponse<Market>(marketId);

        _logger.LogDebug("Successfully handle create market request for market '{marketId}'.", marketId);
        return response;
    }
}
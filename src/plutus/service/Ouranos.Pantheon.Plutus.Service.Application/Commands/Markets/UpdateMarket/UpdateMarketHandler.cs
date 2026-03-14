using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;

namespace Ouranos.Pantheon.Plutus.Service.Application.Commands.Markets.UpdateMarket;

public sealed class UpdateMarketHandler : CommandHandler<UpdateMarketInput, IdResponse<Market>>
{
    private readonly ILogger<UpdateMarketHandler> _logger;
    private readonly IPlutusUnitOfWork _unitOfWork;

    public UpdateMarketHandler(ILogger<UpdateMarketHandler> logger, IPlutusUnitOfWork unitOfWork)
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(unitOfWork);

        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public override async Task<IdResponse<Market>> Handle(
        UpdateMarketInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogDebug("Attempting to handle update market command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var market = await _unitOfWork.Markets.Read(command.MarketId, cancellationToken);
        market.Update(command.Name, command.Taxes);
        await _unitOfWork.Markets.Update(market, cancellationToken);
        await _unitOfWork.SaveChanges(cancellationToken);
        var response = new IdResponse<Market>(market.Id);

        _logger.LogDebug("Successfully handled updated market request.");
        return response;
    }
}

using MediatR;
using Microsoft.Extensions.Logging;
using Talos.Olympus.Core.Application.Common;
using Talos.Olympus.Core.Application.Interfaces.Common;
using Talos.Olympus.Service.Plutus.Domain.Markets;

namespace Talos.Olympus.Service.Plutus.Application.Commands.Markets.UpdateMarket;

public sealed class UpdateMarketHandler : IRequestHandler<UpdateMarketInput, IdResponse<Market>>
{
    private readonly ILogger<UpdateMarketHandler> _logger;
    private readonly ICrudRepository<Market> _marketRepository;

    public UpdateMarketHandler(ILogger<UpdateMarketHandler> logger, ICrudRepository<Market> marketRepository)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(marketRepository);

        _logger = logger;
        _marketRepository = marketRepository;
    }

    public async Task<IdResponse<Market>> Handle(UpdateMarketInput request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Attempting to handle update market request '{@request}'.", request);
        cancellationToken.ThrowIfCancellationRequested();

        var market = await _marketRepository.Read(request.MarketId, cancellationToken);
        market.Update(request.Name, request.Taxes);
        await _marketRepository.Update(market, cancellationToken);

        _logger.LogDebug("Successfully handled updated market request.");
        return new IdResponse<Market>(market.Id);
    }
}
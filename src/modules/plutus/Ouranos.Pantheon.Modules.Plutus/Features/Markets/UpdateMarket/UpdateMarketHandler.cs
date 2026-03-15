using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.UpdateMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.UpdateMarket;

public sealed class UpdateMarketHandler : CommandHandler<UpdateMarketInput, IdResponse<Market>>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<UpdateMarketHandler> _logger;

    public UpdateMarketHandler(
        ILogger<UpdateMarketHandler> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public override async Task<IdResponse<Market>> Handle(
        UpdateMarketInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle update market command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var market = await _dbContext.Markets
            .FirstOrDefaultAsync(m => m.Id == command.MarketId, cancellationToken);

        Guard.Against.NotFound(command.MarketId, market);

        market.Update(
            command.Name,
            command.Taxes
        );

        _dbContext.Markets.Update(market);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var response = new IdResponse<Market>(command.MarketId);

        _logger.LogDebug("Successfully handled update market request for market '{marketId}'.", market.Id);
        return response;
    }
}

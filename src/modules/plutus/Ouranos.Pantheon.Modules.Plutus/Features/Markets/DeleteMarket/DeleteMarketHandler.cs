using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.DeleteMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.DeleteMarket;

public sealed class DeleteMarketHandler : IPantheonHandler<DeleteMarketInput, IdResponse<Market>>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<DeleteMarketHandler> _logger;

    public DeleteMarketHandler(ILogger<DeleteMarketHandler> logger, PlutusDbContext dbContext)
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<IdResponse<Market>> Handle(
        DeleteMarketInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle delete market command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var market = await _dbContext.Markets.FirstOrDefaultAsync(
            m => m.Id == command.MarketId,
            cancellationToken
        );

        Guard.Against.NotFound(command.MarketId, market);

        _dbContext.Markets.Remove(market);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var response = new IdResponse<Market>(command.MarketId);

        _logger.LogDebug("Successfully handled delete market request.");
        return response;
    }
}

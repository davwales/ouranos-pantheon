using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.CreateMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.CreateMarket;

public sealed class CreateMarketHandler : CommandHandler<CreateMarketInput, IdResponse<Market>>
{
    private readonly ILogger<CreateMarketHandler> _logger;
    private readonly PlutusDbContext _dbContext;

    public CreateMarketHandler(
        ILogger<CreateMarketHandler> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public override async Task<IdResponse<Market>> Handle(
        CreateMarketInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle create market command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var market = Market.Create(
            DatabaseExtensions.CreateId<Market>(),
            command.Name,
            command.Taxes,
            command.IsForecastingEnabled,
            command.Description,
            command.Icon
        );

        await _dbContext.Markets.AddAsync(market, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var response = new IdResponse<Market>(market.Id);

        _logger.LogDebug("Successfully handled create market request for market '{marketId}'.", market.Id);
        return response;
    }
}

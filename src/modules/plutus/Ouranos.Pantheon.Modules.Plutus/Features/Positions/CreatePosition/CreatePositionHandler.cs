using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.CreatePosition.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Positions.CreatePosition;

public sealed class CreatePositionHandler : IPantheonHandler<CreatePositionInput, IdResponse<Position>>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<CreatePositionHandler> _logger;

    public CreatePositionHandler(
        ILogger<CreatePositionHandler> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<IdResponse<Position>> Handle(
        CreatePositionInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle create position command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var position = Position.Create(
            command.Side,
            command.MarketId,
            command.SymbolId,
            command.Cost,
            command.Quantity,
            command.StrategyId,
            command.Notes
        );

        await _dbContext.Positions.AddAsync(position, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully handled create position command.");
        return new IdResponse<Position>(position.Id);
    }
}

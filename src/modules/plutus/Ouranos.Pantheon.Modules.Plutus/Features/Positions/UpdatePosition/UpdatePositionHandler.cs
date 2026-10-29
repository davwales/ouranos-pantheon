using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.UpdatePosition.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database.Querying;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Positions.UpdatePosition;

public sealed class UpdatePositionHandler
    : IPantheonHandler<UpdatePositionInput, IdResponse<Position>>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<UpdatePositionHandler> _logger;

    public UpdatePositionHandler(ILogger<UpdatePositionHandler> logger, PlutusDbContext dbContext)
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<IdResponse<Position>> Handle(
        UpdatePositionInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle update position command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var position = await _dbContext.Positions.FirstOrDefaultAsync(
            p => p.Id == command.PositionId,
            cancellationToken
        );

        Guard.Against.NotFound(command.PositionId, position);

        if (position.Status == PositionStatus.Pending)
        {
            position.Modify(command.Cost, command.Quantity, command.Notes, command.TargetPrice);
            await RevalidateLinkAsync(position, cancellationToken);
        }
        else
        {
            position.SetTargetPrice(command.TargetPrice);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully handled update position command.");
        return new IdResponse<Position>(position.Id);
    }

    private async Task RevalidateLinkAsync(Position position, CancellationToken cancellationToken)
    {
        if (position.LinkedBuyPositionId is not { } buyPositionId)
        {
            return;
        }

        var buyPosition = await _dbContext.Positions.FirstOrDefaultAsync(
            p => p.Id == buyPositionId,
            cancellationToken
        );

        Guard.Against.NotFound(buyPositionId, buyPosition);

        var committedSellQuantity = await _dbContext.Positions.CommittedSellQuantityAsync(
            buyPositionId,
            position.Id,
            cancellationToken
        );

        // Re-linking re-checks the changed quantity against what the buy has left.
        position.LinkPosition(buyPosition, committedSellQuantity);
    }
}

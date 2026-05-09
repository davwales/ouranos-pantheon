using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.LinkPosition.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Positions.LinkPosition;

public sealed class LinkPositionHandler
    : IPantheonHandler<LinkPositionInput, IdResponse<Position>>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<LinkPositionHandler> _logger;

    public LinkPositionHandler(
        ILogger<LinkPositionHandler> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<IdResponse<Position>> Handle(
        LinkPositionInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle link position command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var position = await _dbContext.Positions
            .FirstOrDefaultAsync(p => p.Id == command.PositionId, cancellationToken);

        Guard.Against.NotFound(command.PositionId, position);

        var targetPosition = await _dbContext.Positions
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == command.TargetPositionId, cancellationToken);

        Guard.Against.NotFound(command.TargetPositionId, targetPosition);

        if (!targetPosition.CanBeLinkedAsTarget())
        {
            throw new InvalidOperationException(
                $"Cannot link to position '{command.TargetPositionId}' with side '{targetPosition.Side}' and status '{targetPosition.Status}'."
            );
        }

        position.LinkPosition(command.TargetPositionId);

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully handled link position command.");
        return new IdResponse<Position>(position.Id);
    }
}

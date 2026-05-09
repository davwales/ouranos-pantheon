using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.ClosePosition.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Positions.ClosePosition;

public sealed class ClosePositionHandler : IPantheonHandler<ClosePositionInput, ClosePositionResponse>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<ClosePositionHandler> _logger;

    public ClosePositionHandler(
        ILogger<ClosePositionHandler> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<ClosePositionResponse> Handle(
        ClosePositionInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle close position command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var position = await _dbContext.Positions
            .FirstOrDefaultAsync(p => p.Id == command.PositionId, cancellationToken);

        Guard.Against.NotFound(command.PositionId, position);

        position.Close(command.CloseStatus);

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully handled close position command.");
        return new ClosePositionResponse(position.Id, position.Status);
    }
}

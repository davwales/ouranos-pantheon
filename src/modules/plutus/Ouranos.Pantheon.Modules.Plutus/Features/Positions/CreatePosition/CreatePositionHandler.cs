using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.CreatePosition.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Positions.CreatePosition;

public sealed class CreatePositionHandler
    : IPantheonHandler<CreatePositionInput, IdResponse<Position>>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<CreatePositionHandler> _logger;

    public CreatePositionHandler(ILogger<CreatePositionHandler> logger, PlutusDbContext dbContext)
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

        await EnsureReferencesBelongToMarketAsync(command, cancellationToken);

        var position = Position.Create(
            command.Side,
            command.MarketId,
            command.SymbolId,
            command.Cost,
            command.Quantity,
            command.StrategyId,
            command.Notes,
            targetPrice: command.TargetPrice
        );

        await _dbContext.Positions.AddAsync(position, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully handled create position command.");
        return new IdResponse<Position>(position.Id);
    }

    private async Task EnsureReferencesBelongToMarketAsync(
        CreatePositionInput command,
        CancellationToken cancellationToken
    )
    {
        var symbolInMarket = await _dbContext.Symbols.AnyAsync(
            s => s.Id == command.SymbolId && s.MarketId == command.MarketId,
            cancellationToken
        );

        if (!symbolInMarket)
        {
            throw new BadRequestException(
                $"Symbol '{command.SymbolId}' does not exist in market '{command.MarketId}'."
            );
        }

        if (command.StrategyId is not { } strategyId)
        {
            return;
        }

        var strategyInMarket = await _dbContext.Strategies.AnyAsync(
            s => s.Id == strategyId && s.MarketId == command.MarketId,
            cancellationToken
        );

        if (!strategyInMarket)
        {
            throw new BadRequestException(
                $"Strategy '{strategyId}' does not exist in market '{command.MarketId}'."
            );
        }
    }
}

using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.DeleteSymbolGroup.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.SymbolGroups;

namespace Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.DeleteSymbolGroup;

public sealed class DeleteSymbolGroupHandler : IPantheonHandler<DeleteSymbolGroupInput, IdResponse<SymbolGroup>>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<DeleteSymbolGroupHandler> _logger;

    public DeleteSymbolGroupHandler(
        ILogger<DeleteSymbolGroupHandler> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<IdResponse<SymbolGroup>> Handle(
        DeleteSymbolGroupInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle delete symbol group command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        await _dbContext.SymbolGroups
            .Where(sg => sg.Id == command.SymbolGroupId)
            .ExecuteDeleteAsync(cancellationToken);

        _logger.LogDebug("Successfully handled delete symbol group command.");
        return new IdResponse<SymbolGroup>(command.SymbolGroupId);
    }
}

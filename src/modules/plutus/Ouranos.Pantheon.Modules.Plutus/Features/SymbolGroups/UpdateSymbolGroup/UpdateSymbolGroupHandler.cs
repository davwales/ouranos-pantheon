using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.UpdateSymbolGroup.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.SymbolGroups;

namespace Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.UpdateSymbolGroup;

public sealed class UpdateSymbolGroupHandler : IPantheonHandler<UpdateSymbolGroupInput, IdResponse<SymbolGroup>>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<UpdateSymbolGroupHandler> _logger;

    public UpdateSymbolGroupHandler(
        ILogger<UpdateSymbolGroupHandler> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<IdResponse<SymbolGroup>> Handle(
        UpdateSymbolGroupInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle update symbol group command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var group = await _dbContext.SymbolGroups
            .FirstOrDefaultAsync(sg => sg.Id == command.SymbolGroupId, cancellationToken);

        Guard.Against.NotFound(command.SymbolGroupId, group);

        group.Update(command.Name, command.Description);

        var existingMembers = await _dbContext.SymbolGroupMembers
            .Where(m => m.SymbolGroupId == command.SymbolGroupId)
            .ToListAsync(cancellationToken);

        var existingIds = existingMembers.Select(m => m.SymbolId).ToHashSet();
        var desiredIds = command.SymbolIds.ToHashSet();

        _dbContext.SymbolGroupMembers.RemoveRange(
            existingMembers.Where(m => !desiredIds.Contains(m.SymbolId))
        );

        _dbContext.SymbolGroupMembers.AddRange(
            desiredIds
                .Where(id => !existingIds.Contains(id))
                .Select(id => SymbolGroupMember.Create(command.SymbolGroupId, id))
        );

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully handled update symbol group command.");
        return new IdResponse<SymbolGroup>(group.Id);
    }
}

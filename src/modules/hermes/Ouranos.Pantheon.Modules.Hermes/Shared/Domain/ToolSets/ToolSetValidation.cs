using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;

public static class ToolSetValidation
{
    public static async Task ValidateToolSetExistsAsync(
        HermesDbContext dbContext,
        Id<ToolSet>? toolSetId,
        CancellationToken cancellationToken = default
    )
    {
        if (toolSetId is null)
        {
            return;
        }

        var exists = await dbContext.ToolSets.AnyAsync(s => s.Id == toolSetId, cancellationToken);

        if (!exists)
        {
            Guard.Against.NotFound(toolSetId.Value, (ToolSet?)null);
        }
    }
}

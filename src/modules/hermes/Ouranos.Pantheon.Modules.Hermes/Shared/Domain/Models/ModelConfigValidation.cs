using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;

public static class ModelConfigValidation
{
    public static async Task ValidateModelConfigExistsAsync(
        HermesDbContext dbContext,
        Id<ModelConfig> modelConfigId,
        CancellationToken cancellationToken = default
    )
    {
        var exists = await dbContext.ModelConfigs.AnyAsync(
            m => m.Id == modelConfigId,
            cancellationToken
        );

        if (!exists)
        {
            Guard.Against.NotFound(modelConfigId, (ModelConfig?)null, "model");
        }
    }

    public static async Task ValidateModelConfigIsUnusedAsync(
        HermesDbContext dbContext,
        ModelConfig modelConfig,
        CancellationToken cancellationToken = default
    )
    {
        var conversationCount = await dbContext.Conversations.CountAsync(
            c => c.ModelConfigId == modelConfig.Id,
            cancellationToken
        );

        if (conversationCount > 0)
        {
            throw new ConflictException(
                $"The model '{modelConfig.Name}' is used by {conversationCount} "
                    + $"conversation{(conversationCount == 1 ? string.Empty : "s")}. "
                    + "Switch them to another model or delete them first."
            );
        }
    }
}

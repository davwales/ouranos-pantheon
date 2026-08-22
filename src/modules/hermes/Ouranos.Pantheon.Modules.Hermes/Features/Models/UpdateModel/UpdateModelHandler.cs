using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.UpdateModel.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Models.UpdateModel;

public sealed class UpdateModelHandler : IPantheonHandler<UpdateModelInput, UpdateModelResponse>
{
    private readonly HermesDbContext _dbContext;
    private readonly ILogger<UpdateModelHandler> _logger;

    public UpdateModelHandler(ILogger<UpdateModelHandler> logger, HermesDbContext dbContext)
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<UpdateModelResponse> Handle(
        UpdateModelInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle update model command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var model = await _dbContext.ModelConfigs.FirstOrDefaultAsync(
            m => m.Id == command.ModelId,
            cancellationToken
        );

        Guard.Against.NotFound(command.ModelId, model);

        if (command.IsDefault)
        {
            var existingDefaults = await _dbContext
                .ModelConfigs.Where(m => m.IsDefault && m.Id != command.ModelId)
                .ToListAsync(cancellationToken);

            foreach (var existing in existingDefaults)
            {
                existing.Update(
                    existing.Name,
                    existing.ModelIdentifier,
                    existing.SystemPrompt,
                    existing.Temperature,
                    existing.MaxTokens,
                    existing.RepeatPenalty,
                    existing.ContextWindow,
                    false,
                    existing.IsPublic
                );
            }
        }

        model.Update(
            command.Name,
            command.ModelIdentifier,
            command.SystemPrompt,
            command.Temperature,
            command.MaxTokens,
            command.RepeatPenalty,
            command.ContextWindow,
            command.IsDefault,
            command.IsPublic
        );

        var isAvailable = await _dbContext.AvailableModels.AnyAsync(
            m => m.ModelIdentifier == command.ModelIdentifier,
            cancellationToken
        );

        if (isAvailable)
        {
            model.MarkAvailable();
        }
        else
        {
            model.MarkUnavailable();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully handled update model request.");
        return new UpdateModelResponse(command.ModelId);
    }
}

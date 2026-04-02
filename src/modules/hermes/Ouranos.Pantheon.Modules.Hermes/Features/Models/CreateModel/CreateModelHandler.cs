using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.CreateModel.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Extensions;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Models.CreateModel;

public sealed class CreateModelHandler : IPantheonHandler<CreateModelInput, CreateModelResponse>
{
    private readonly ILogger<CreateModelHandler> _logger;
    private readonly HermesDbContext _dbContext;

    public CreateModelHandler(
        ILogger<CreateModelHandler> logger,
        HermesDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<CreateModelResponse> Handle(
        CreateModelInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle create model command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        if (command.IsDefault)
        {
            var existingDefaults = await _dbContext.ModelConfigs
                .Where(m => m.IsDefault)
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
                    false,
                    existing.IsPublic
                );
            }
        }

        var model = ModelConfig.Create(
            DatabaseExtensions.CreateId<ModelConfig>(),
            command.Name,
            command.ModelIdentifier,
            command.SystemPrompt,
            command.Temperature,
            command.MaxTokens,
            command.RepeatPenalty,
            command.IsDefault,
            command.IsPublic
        );

        await _dbContext.ModelConfigs.AddAsync(model, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully handled create model request for model '{modelId}'.", model.Id);
        return new CreateModelResponse(model.Id);
    }
}

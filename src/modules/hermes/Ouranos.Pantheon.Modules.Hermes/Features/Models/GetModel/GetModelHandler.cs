using Ardalis.GuardClauses;
using Flagsmith;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.GetModel.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Shared.Application;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Models.GetModel;

public sealed class GetModelHandler : IPantheonHandler<GetModelInput, GetModelResponse>
{
    private readonly HermesDbContext _dbContext;
    private readonly IFlagsmithClient _flagsmith;
    private readonly ILogger<GetModelHandler> _logger;

    public GetModelHandler(
        ILogger<GetModelHandler> logger,
        HermesDbContext dbContext,
        IFlagsmithClient flagsmith
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);
        Guard.Against.Null(flagsmith);

        _logger = logger;
        _dbContext = dbContext;
        _flagsmith = flagsmith;
    }

    public async Task<GetModelResponse> Handle(
        GetModelInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get model query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var model = await _dbContext.ModelConfigs.FirstOrDefaultAsync(
            m => m.Id == query.ModelId,
            cancellationToken
        );

        Guard.Against.NotFound(query.ModelId, model);

        if (!model.IsPublic)
        {
            var flags = await _flagsmith.GetEnvironmentFlags();
            if (await flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode))
            {
                Guard.Against.NotFound(query.ModelId, (ModelConfig?)null);
            }
        }

        _logger.LogDebug("Successfully handled get model request.");
        return new GetModelResponse(
            model.Id,
            model.Name,
            model.ModelIdentifier,
            model.SystemPrompt,
            model.Temperature,
            model.MaxTokens,
            model.RepeatPenalty,
            model.ContextWindow,
            model.IsDefault,
            model.IsPublic,
            model.IsUnavailable
        );
    }
}

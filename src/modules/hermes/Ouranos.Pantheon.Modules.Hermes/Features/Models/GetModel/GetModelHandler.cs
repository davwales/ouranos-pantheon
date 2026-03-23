using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.GetModel.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Models.GetModel;

public sealed class GetModelHandler : IPantheonHandler<GetModelInput, GetModelResponse>
{
    private readonly HermesDbContext _dbContext;
    private readonly ILogger<GetModelHandler> _logger;

    public GetModelHandler(
        ILogger<GetModelHandler> logger,
        HermesDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<GetModelResponse> Handle(
        GetModelInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get model query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var model = await _dbContext.ModelConfigs
            .FirstOrDefaultAsync(m => m.Id == query.ModelId, cancellationToken);

        Guard.Against.NotFound(query.ModelId, model);

        _logger.LogDebug("Successfully handled get model request.");
        return new GetModelResponse(
            model.Id,
            model.Name,
            model.ModelIdentifier,
            model.SystemPrompt,
            model.Temperature,
            model.MaxTokens,
            model.RepeatPenalty,
            model.IsDefault
        );
    }
}

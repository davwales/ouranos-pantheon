using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Sorting;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.GetAllModels.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Models.GetAllModels;

public sealed class GetAllModelsHandler
    : IPantheonHandler<GetAllModelsInput, List<GetAllModelsResponse>>
{
    private static readonly FilterBuilder<ModelConfig> FilterBuilder = new FilterBuilder<ModelConfig>()
        .On(nameof(ModelConfig.Name), m => m.Name)
        .On(nameof(ModelConfig.ModelIdentifier), m => m.ModelIdentifier);

    private static readonly SortBuilder<ModelConfig> SortBuilder = new SortBuilder<ModelConfig>()
        .On(nameof(ModelConfig.Name), m => m.Name)
        .On(nameof(ModelConfig.ModelIdentifier), m => m.ModelIdentifier)
        .On(nameof(ModelConfig.MaxTokens), m => m.MaxTokens)
        .On(nameof(ModelConfig.Temperature), m => m.Temperature)
        .On(nameof(ModelConfig.RepeatPenalty), m => m.RepeatPenalty)
        .Default(m => m.Name, SortDirection.Asc);

    private readonly HermesDbContext _dbContext;
    private readonly ILogger<GetAllModelsHandler> _logger;

    public GetAllModelsHandler(
        ILogger<GetAllModelsHandler> logger,
        HermesDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<List<GetAllModelsResponse>> Handle(
        GetAllModelsInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get all models query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var models = await _dbContext.ModelConfigs
            .AsQueryable()
            .AsNoTracking()
            .FilterBy(query.Filter, FilterBuilder)
            .SortBy(query.SortField, query.SortDirection, SortBuilder)
            .Select(m => new GetAllModelsResponse(
                    m.Id,
                    m.Name,
                    m.ModelIdentifier,
                    m.SystemPrompt,
                    m.Temperature,
                    m.MaxTokens,
                    m.RepeatPenalty,
                    m.IsDefault
                )
            )
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Successfully handled get all models request.");
        return models;
    }
}

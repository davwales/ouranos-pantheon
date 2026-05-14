using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.GetAvailableModels.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.AvailableModels;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Sorting;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Models.GetAvailableModels;

public sealed class GetAvailableModelsHandler
    : IPantheonHandler<GetAvailableModelsInput, List<GetAvailableModelsResponse>>
{
    private static readonly FilterBuilder<AvailableModel> FilterBuilder =
        new FilterBuilder<AvailableModel>()
            .On(
                nameof(AvailableModel.ModelIdentifier),
                m => m.ModelIdentifier,
                caseInsensitive: true
            )
            .On(nameof(AvailableModel.OwnedBy), m => m.OwnedBy, caseInsensitive: true);

    private static readonly SortBuilder<AvailableModel> SortBuilder =
        new SortBuilder<AvailableModel>()
            .On(nameof(AvailableModel.ModelIdentifier), m => m.ModelIdentifier)
            .On(nameof(AvailableModel.OwnedBy), m => m.OwnedBy)
            .Default(m => m.ModelIdentifier, SortDirection.Asc);

    private readonly HermesDbContext _dbContext;
    private readonly ILogger<GetAvailableModelsHandler> _logger;

    public GetAvailableModelsHandler(
        ILogger<GetAvailableModelsHandler> logger,
        HermesDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<List<GetAvailableModelsResponse>> Handle(
        GetAvailableModelsInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get available models query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var models = await _dbContext
            .AvailableModels.AsQueryable()
            .AsNoTracking()
            .FilterBy(query.Filter, FilterBuilder)
            .SortBy(query.SortField, query.SortDirection, SortBuilder)
            .Select(m => new GetAvailableModelsResponse(m.Id, m.ModelIdentifier, m.OwnedBy))
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Successfully handled get available models request.");
        return models;
    }
}

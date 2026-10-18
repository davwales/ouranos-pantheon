using Ardalis.GuardClauses;
using Flagsmith;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetAllToolSets.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Filtering;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Sorting;

namespace Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetAllToolSets;

public sealed class GetAllToolSetsHandler
    : IPantheonHandler<GetAllToolSetsInput, List<GetAllToolSetsResponse>>
{
    private static readonly FilterBuilder<ToolSet> FilterBuilder = new FilterBuilder<ToolSet>()
        .On(nameof(ToolSet.Name), s => s.Name, caseInsensitive: true)
        .On(nameof(ToolSet.IsPublic), s => s.IsPublic);

    private static readonly SortBuilder<ToolSet> SortBuilder = new SortBuilder<ToolSet>()
        .On(nameof(ToolSet.Name), s => s.Name)
        .Default(s => s.Name, SortDirection.Asc);

    private readonly HermesDbContext _dbContext;
    private readonly IFlagsmithClient _flagsmith;
    private readonly ILogger<GetAllToolSetsHandler> _logger;
    private readonly IReadOnlyList<IHermesToolSource> _toolSources;

    public GetAllToolSetsHandler(
        ILogger<GetAllToolSetsHandler> logger,
        HermesDbContext dbContext,
        IFlagsmithClient flagsmith,
        IEnumerable<IHermesToolSource> toolSources
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);
        Guard.Against.Null(flagsmith);
        Guard.Against.Null(toolSources);

        _logger = logger;
        _dbContext = dbContext;
        _flagsmith = flagsmith;
        _toolSources = [.. toolSources];
    }

    public async Task<List<GetAllToolSetsResponse>> Handle(
        GetAllToolSetsInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get all tool sets query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var flags = await _flagsmith.GetEnvironmentFlags();
        var isPublicMode = await flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode);

        var dbQuery = _dbContext.ToolSets.AsQueryable().AsNoTracking();

        if (isPublicMode)
        {
            dbQuery = dbQuery.Where(s => s.IsPublic);
        }

        var toolSets = await dbQuery
            .FilterBy(query.Filter, FilterBuilder)
            .SortBy(query.SortField, query.SortDirection, SortBuilder)
            .Include(s => s.Tools)
            .ToListAsync(cancellationToken);

        var available = _toolSources.ListAllTools();

        _logger.LogDebug("Successfully handled get all tool sets request.");
        return
        [
            .. toolSets.Select(s => new GetAllToolSetsResponse(
                s.Id,
                s.Name,
                s.Description,
                [
                    .. s
                        .Tools.OrderBy(t => t.Source)
                        .ThenBy(t => t.Name, StringComparer.Ordinal)
                        .Select(t => new GetAllToolSetsToolResponse(
                            t.Source,
                            t.Name,
                            t.AdditionalDescription,
                            available.Includes(t.Source, t.Name)
                        )),
                ],
                s.IsPublic
            )),
        ];
    }
}

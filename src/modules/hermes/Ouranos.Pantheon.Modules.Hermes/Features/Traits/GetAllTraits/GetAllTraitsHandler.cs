using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Sorting;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.GetAllTraits.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Flagsmith;
using Trait = Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits.Trait;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Traits.GetAllTraits;

public sealed class GetAllTraitsHandler
    : IPantheonHandler<GetAllTraitsInput, List<GetAllTraitsResponse>>
{
    private static readonly FilterBuilder<Trait> FilterBuilder = new FilterBuilder<Trait>()
        .On(nameof(Trait.Name), t => t.Name, caseInsensitive: true)
        .On(nameof(Trait.Content), t => t.Content, caseInsensitive: true)
        .On(nameof(Trait.IsPublic), t => t.IsPublic);

    private static readonly SortBuilder<Trait> SortBuilder = new SortBuilder<Trait>()
        .On(nameof(Trait.Name), t => t.Name)
        .Default(t => t.Name, SortDirection.Asc);

    private readonly HermesDbContext _dbContext;
    private readonly IFlagsmithClient _flagsmith;
    private readonly ILogger<GetAllTraitsHandler> _logger;

    public GetAllTraitsHandler(
        ILogger<GetAllTraitsHandler> logger,
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

    public async Task<List<GetAllTraitsResponse>> Handle(
        GetAllTraitsInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get all traits query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var flags = await _flagsmith.GetEnvironmentFlags();
        var isPublicMode = await flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode);

        var dbQuery = _dbContext.Traits
            .AsQueryable()
            .AsNoTracking();

        if (isPublicMode)
        {
            dbQuery = dbQuery.Where(t => t.IsPublic);
        }

        var traits = await dbQuery
            .FilterBy(query.Filter, FilterBuilder)
            .SortBy(query.SortField, query.SortDirection, SortBuilder)
            .Select(t => new GetAllTraitsResponse(
                    t.Id,
                    t.Name,
                    t.Content,
                    t.IsPublic
                )
            )
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Successfully handled get all traits request.");
        return traits;
    }
}

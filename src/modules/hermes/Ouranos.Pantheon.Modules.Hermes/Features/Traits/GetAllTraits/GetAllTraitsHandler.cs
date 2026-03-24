using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Sorting;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.GetAllTraits.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Traits.GetAllTraits;

public sealed class GetAllTraitsHandler
    : IPantheonHandler<GetAllTraitsInput, List<GetAllTraitsResponse>>
{
    private static readonly FilterBuilder<Trait> FilterBuilder = new FilterBuilder<Trait>()
        .On(nameof(Trait.Name), t => t.Name)
        .On(nameof(Trait.Content), t => t.Content);

    private static readonly SortBuilder<Trait> SortBuilder = new SortBuilder<Trait>()
        .On(nameof(Trait.Name), t => t.Name)
        .Default(t => t.Name, SortDirection.Asc);

    private readonly HermesDbContext _dbContext;
    private readonly ILogger<GetAllTraitsHandler> _logger;

    public GetAllTraitsHandler(
        ILogger<GetAllTraitsHandler> logger,
        HermesDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<List<GetAllTraitsResponse>> Handle(
        GetAllTraitsInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get all traits query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var traits = await _dbContext.Traits
            .AsQueryable()
            .AsNoTracking()
            .FilterBy(query.Filter, FilterBuilder)
            .SortBy(query.SortField, query.SortDirection, SortBuilder)
            .Select(t => new GetAllTraitsResponse(
                    t.Id,
                    t.Name,
                    t.Content
                )
            )
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Successfully handled get all traits request.");
        return traits;
    }
}

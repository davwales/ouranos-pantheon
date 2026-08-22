using Ardalis.GuardClauses;
using Flagsmith;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.GetAllPersonas.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Filtering;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Sorting;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Personas.GetAllPersonas;

public sealed class GetAllPersonasHandler
    : IPantheonHandler<GetAllPersonasInput, List<GetAllPersonasResponse>>
{
    private static readonly FilterBuilder<Persona> FilterBuilder = new FilterBuilder<Persona>()
        .On(nameof(Persona.Name), p => p.Name, caseInsensitive: true)
        .On(nameof(Persona.IsPublic), p => p.IsPublic);

    private static readonly SortBuilder<Persona> SortBuilder = new SortBuilder<Persona>()
        .On(nameof(Persona.Name), p => p.Name)
        .On(nameof(Persona.IsDefault), p => p.IsDefault)
        .Default(p => p.Name, SortDirection.Asc);

    private readonly HermesDbContext _dbContext;
    private readonly IFlagsmithClient _flagsmith;
    private readonly ILogger<GetAllPersonasHandler> _logger;

    public GetAllPersonasHandler(
        ILogger<GetAllPersonasHandler> logger,
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

    public async Task<List<GetAllPersonasResponse>> Handle(
        GetAllPersonasInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get all personas query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var flags = await _flagsmith.GetEnvironmentFlags();
        var isPublicMode = await flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode);

        var dbQuery = _dbContext.Personas.AsQueryable().AsNoTracking();

        if (isPublicMode)
        {
            dbQuery = dbQuery.Where(p => p.IsPublic);
        }

        var personas = await dbQuery
            .FilterBy(query.Filter, FilterBuilder)
            .SortBy(query.SortField, query.SortDirection, SortBuilder)
            .Select(p => new GetAllPersonasResponse(
                p.Id,
                p.Name,
                p.Description,
                p.Personality,
                p.Scenario,
                p.IsDefault,
                p.IsPublic
            ))
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Successfully handled get all personas request.");
        return personas;
    }
}

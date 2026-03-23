using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Sorting;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.GetAllPersonas.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Personas.GetAllPersonas;

public sealed class GetAllPersonasHandler
    : IPantheonHandler<GetAllPersonasInput, List<GetAllPersonasResponse>>
{
    private static readonly FilterBuilder<Persona> FilterBuilder = new FilterBuilder<Persona>()
        .On(nameof(Persona.Name), p => p.Name);

    private static readonly SortBuilder<Persona> SortBuilder = new SortBuilder<Persona>()
        .On(nameof(Persona.Name), p => p.Name)
        .On(nameof(Persona.IsDefault), p => p.IsDefault)
        .Default(p => p.Name, SortDirection.Asc);

    private readonly HermesDbContext _dbContext;
    private readonly ILogger<GetAllPersonasHandler> _logger;

    public GetAllPersonasHandler(
        ILogger<GetAllPersonasHandler> logger,
        HermesDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<List<GetAllPersonasResponse>> Handle(
        GetAllPersonasInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get all personas query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var personas = await _dbContext.Personas
            .AsQueryable()
            .AsNoTracking()
            .FilterBy(query.Filter, FilterBuilder)
            .SortBy(query.SortField, query.SortDirection, SortBuilder)
            .Select(p => new GetAllPersonasResponse(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Personality,
                    p.Scenario,
                    p.IsDefault
                )
            )
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Successfully handled get all personas request.");
        return personas;
    }
}

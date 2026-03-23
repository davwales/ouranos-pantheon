using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.GetPersona.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Personas.GetPersona;

public sealed class GetPersonaHandler : IPantheonHandler<GetPersonaInput, GetPersonaResponse>
{
    private readonly HermesDbContext _dbContext;
    private readonly ILogger<GetPersonaHandler> _logger;

    public GetPersonaHandler(
        ILogger<GetPersonaHandler> logger,
        HermesDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<GetPersonaResponse> Handle(
        GetPersonaInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get persona query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var persona = await _dbContext.Personas
            .FirstOrDefaultAsync(p => p.Id == query.PersonaId, cancellationToken);

        Guard.Against.NotFound(query.PersonaId, persona);

        _logger.LogDebug("Successfully handled get persona request.");
        return new GetPersonaResponse(
            persona.Id,
            persona.Name,
            persona.Description,
            persona.Personality,
            persona.Scenario,
            persona.IsDefault
        );
    }
}

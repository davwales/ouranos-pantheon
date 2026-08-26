using Ardalis.GuardClauses;
using Flagsmith;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.GetPersona.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Personas.GetPersona;

public sealed class GetPersonaHandler : IPantheonHandler<GetPersonaInput, GetPersonaResponse>
{
    private readonly HermesDbContext _dbContext;
    private readonly IFlagsmithClient _flagsmith;
    private readonly ILogger<GetPersonaHandler> _logger;

    public GetPersonaHandler(
        ILogger<GetPersonaHandler> logger,
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

    public async Task<GetPersonaResponse> Handle(
        GetPersonaInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get persona query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var persona = await _dbContext.Personas.FirstOrDefaultAsync(
            p => p.Id == query.PersonaId,
            cancellationToken
        );

        Guard.Against.NotFound(query.PersonaId, persona);

        if (!persona.IsPublic)
        {
            var flags = await _flagsmith.GetEnvironmentFlags();
            if (await flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode))
            {
                Guard.Against.NotFound(query.PersonaId, (Persona?)null);
            }
        }

        _logger.LogDebug("Successfully handled get persona request.");
        return new GetPersonaResponse(
            persona.Id,
            persona.Name,
            persona.Description,
            persona.Personality,
            persona.Scenario,
            persona.IsDefault,
            persona.IsPublic
        );
    }
}

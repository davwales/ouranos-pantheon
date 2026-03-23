using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.UpdatePersona.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Personas.UpdatePersona;

public sealed class UpdatePersonaHandler : IPantheonHandler<UpdatePersonaInput, UpdatePersonaResponse>
{
    private readonly HermesDbContext _dbContext;
    private readonly ILogger<UpdatePersonaHandler> _logger;

    public UpdatePersonaHandler(
        ILogger<UpdatePersonaHandler> logger,
        HermesDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<UpdatePersonaResponse> Handle(
        UpdatePersonaInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle update persona command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var persona = await _dbContext.Personas
            .FirstOrDefaultAsync(p => p.Id == command.PersonaId, cancellationToken);

        Guard.Against.NotFound(command.PersonaId, persona);

        if (command.IsDefault)
        {
            await _dbContext.Personas
                .Where(p => p.IsDefault && p.Id != command.PersonaId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsDefault, false), cancellationToken);
        }

        persona.Update(
            command.Name,
            command.Description,
            command.Personality,
            command.Scenario,
            command.IsDefault
        );

        _dbContext.Personas.Update(persona);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully handled update persona request.");
        return new UpdatePersonaResponse(command.PersonaId);
    }
}

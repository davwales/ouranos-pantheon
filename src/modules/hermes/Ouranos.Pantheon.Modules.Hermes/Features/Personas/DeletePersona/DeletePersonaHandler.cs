using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.DeletePersona.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Personas.DeletePersona;

public sealed class DeletePersonaHandler : IPantheonHandler<DeletePersonaInput, DeletePersonaResponse>
{
    private readonly HermesDbContext _dbContext;
    private readonly ILogger<DeletePersonaHandler> _logger;

    public DeletePersonaHandler(
        ILogger<DeletePersonaHandler> logger,
        HermesDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<DeletePersonaResponse> Handle(
        DeletePersonaInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle delete persona command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var persona = await _dbContext.Personas
            .FirstOrDefaultAsync(p => p.Id == command.PersonaId, cancellationToken);

        Guard.Against.NotFound(command.PersonaId, persona);

        _dbContext.Personas.Remove(persona);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully handled delete persona request.");
        return new DeletePersonaResponse(command.PersonaId);
    }
}

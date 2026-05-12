using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.CreatePersona.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Extensions;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Personas.CreatePersona;

public sealed class CreatePersonaHandler
    : IPantheonHandler<CreatePersonaInput, CreatePersonaResponse>
{
    private readonly ILogger<CreatePersonaHandler> _logger;
    private readonly HermesDbContext _dbContext;

    public CreatePersonaHandler(ILogger<CreatePersonaHandler> logger, HermesDbContext dbContext)
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<CreatePersonaResponse> Handle(
        CreatePersonaInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle create persona command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        if (command.IsDefault)
        {
            var existingDefaults = await _dbContext
                .Personas.Where(p => p.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var existing in existingDefaults)
            {
                existing.Update(
                    existing.Name,
                    existing.Description,
                    existing.Personality,
                    existing.Scenario,
                    false,
                    existing.IsPublic
                );
            }
        }

        var persona = Persona.Create(
            DatabaseExtensions.CreateId<Persona>(),
            command.Name,
            command.Description,
            command.Personality,
            command.Scenario,
            command.IsDefault,
            command.IsPublic
        );

        await _dbContext.Personas.AddAsync(persona, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Successfully handled create persona request for persona '{personaId}'.",
            persona.Id
        );
        return new CreatePersonaResponse(persona.Id);
    }
}

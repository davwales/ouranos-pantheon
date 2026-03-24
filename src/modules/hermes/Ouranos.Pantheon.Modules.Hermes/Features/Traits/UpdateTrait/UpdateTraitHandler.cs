using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.UpdateTrait.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Traits.UpdateTrait;

public sealed class UpdateTraitHandler : IPantheonHandler<UpdateTraitInput, UpdateTraitResponse>
{
    private readonly HermesDbContext _dbContext;
    private readonly ILogger<UpdateTraitHandler> _logger;

    public UpdateTraitHandler(
        ILogger<UpdateTraitHandler> logger,
        HermesDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<UpdateTraitResponse> Handle(
        UpdateTraitInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle update trait command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var trait = await _dbContext.Traits
            .FirstOrDefaultAsync(t => t.Id == command.TraitId, cancellationToken);

        Guard.Against.NotFound(command.TraitId, trait);

        trait.Update(
            command.Name,
            command.Content
        );

        _dbContext.Traits.Update(trait);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully handled update trait request.");
        return new UpdateTraitResponse(command.TraitId);
    }
}

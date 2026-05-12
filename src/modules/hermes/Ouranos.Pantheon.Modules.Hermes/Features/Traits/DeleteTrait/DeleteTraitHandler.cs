using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.DeleteTrait.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Application;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Traits.DeleteTrait;

public sealed class DeleteTraitHandler : IPantheonHandler<DeleteTraitInput, DeleteTraitResponse>
{
    private readonly HermesDbContext _dbContext;
    private readonly ILogger<DeleteTraitHandler> _logger;

    public DeleteTraitHandler(ILogger<DeleteTraitHandler> logger, HermesDbContext dbContext)
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<DeleteTraitResponse> Handle(
        DeleteTraitInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle delete trait command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var trait = await _dbContext.Traits.FirstOrDefaultAsync(
            t => t.Id == command.TraitId,
            cancellationToken
        );

        Guard.Against.NotFound(command.TraitId, trait);

        _dbContext.Traits.Remove(trait);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully handled delete trait request.");
        return new DeleteTraitResponse(command.TraitId);
    }
}

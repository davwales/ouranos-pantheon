using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.DeleteTrait.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Traits.DeleteTrait;

public sealed class DeleteTraitHandler : IPantheonHandler<DeleteTraitInput, DeleteTraitResponse>
{
    private readonly HermesDbContext _dbContext;
    private readonly ILogger<DeleteTraitHandler> _logger;

    public DeleteTraitHandler(
        ILogger<DeleteTraitHandler> logger,
        HermesDbContext dbContext
    )
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

        await _dbContext.Traits
            .Where(t => t.Id == command.TraitId)
            .ExecuteDeleteAsync(cancellationToken);

        _logger.LogDebug("Successfully handled delete trait request.");
        return new DeleteTraitResponse(command.TraitId);
    }
}

using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.GetTrait.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Traits.GetTrait;

public sealed class GetTraitHandler : IPantheonHandler<GetTraitInput, GetTraitResponse>
{
    private readonly HermesDbContext _dbContext;
    private readonly ILogger<GetTraitHandler> _logger;

    public GetTraitHandler(
        ILogger<GetTraitHandler> logger,
        HermesDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<GetTraitResponse> Handle(
        GetTraitInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get trait query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var trait = await _dbContext.Traits
            .FirstOrDefaultAsync(t => t.Id == query.TraitId, cancellationToken);

        Guard.Against.NotFound(query.TraitId, trait);

        _logger.LogDebug("Successfully handled get trait request.");
        return new GetTraitResponse(
            trait.Id,
            trait.Name,
            trait.Content
        );
    }
}

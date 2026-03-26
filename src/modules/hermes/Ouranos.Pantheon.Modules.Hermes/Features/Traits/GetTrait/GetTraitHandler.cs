using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.GetTrait.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Flagsmith;
using Trait = Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits.Trait;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Traits.GetTrait;

public sealed class GetTraitHandler : IPantheonHandler<GetTraitInput, GetTraitResponse>
{
    private readonly HermesDbContext _dbContext;
    private readonly IFlagsmithClient _flagsmith;
    private readonly ILogger<GetTraitHandler> _logger;

    public GetTraitHandler(
        ILogger<GetTraitHandler> logger,
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

        if (!trait.IsPublic)
        {
            var flags = await _flagsmith.GetEnvironmentFlags();
            if (await flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode))
            {
                Guard.Against.NotFound(query.TraitId, (Trait?)null);
            }
        }

        _logger.LogDebug("Successfully handled get trait request.");
        return new GetTraitResponse(
            trait.Id,
            trait.Name,
            trait.Content,
            trait.IsPublic
        );
    }
}

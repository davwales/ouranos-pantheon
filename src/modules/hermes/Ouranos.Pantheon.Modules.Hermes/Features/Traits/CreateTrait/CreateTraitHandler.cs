using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.CreateTrait.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Traits.CreateTrait;

public sealed class CreateTraitHandler : IPantheonHandler<CreateTraitInput, CreateTraitResponse>
{
    private readonly ILogger<CreateTraitHandler> _logger;
    private readonly HermesDbContext _dbContext;

    public CreateTraitHandler(ILogger<CreateTraitHandler> logger, HermesDbContext dbContext)
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<CreateTraitResponse> Handle(
        CreateTraitInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle create trait command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var trait = Trait.Create(
            DatabaseExtensions.CreateId<Trait>(),
            command.Name,
            command.Content,
            command.IsPublic
        );

        await _dbContext.Traits.AddAsync(trait, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Successfully handled create trait request for trait '{traitId}'.",
            trait.Id
        );
        return new CreateTraitResponse(trait.Id);
    }
}

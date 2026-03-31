using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Extensions;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.CreateSymbolGroup.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.SymbolGroups;

namespace Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.CreateSymbolGroup;

public sealed class CreateSymbolGroupHandler : IPantheonHandler<CreateSymbolGroupInput, IdResponse<SymbolGroup>>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<CreateSymbolGroupHandler> _logger;

    public CreateSymbolGroupHandler(
        ILogger<CreateSymbolGroupHandler> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<IdResponse<SymbolGroup>> Handle(
        CreateSymbolGroupInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle create symbol group command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var group = SymbolGroup.Create(
            DatabaseExtensions.CreateId<SymbolGroup>(),
            command.Name,
            command.Description,
            command.MarketId
        );

        await _dbContext.SymbolGroups.AddAsync(group, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully handled create symbol group command.");
        return new IdResponse<SymbolGroup>(group.Id);
    }
}

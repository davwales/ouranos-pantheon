using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.DeleteLora.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Loras.DeleteLora;

public sealed class DeleteLoraHandler : IPantheonHandler<DeleteLoraInput, DeleteLoraResponse>
{
    private readonly HermesDbContext _dbContext;
    private readonly ILogger<DeleteLoraHandler> _logger;

    public DeleteLoraHandler(ILogger<DeleteLoraHandler> logger, HermesDbContext dbContext)
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<DeleteLoraResponse> Handle(
        DeleteLoraInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle delete LoRA command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var lora = await _dbContext.Loras.FirstOrDefaultAsync(
            l => l.Id == command.LoraId,
            cancellationToken
        );

        Guard.Against.NotFound(command.LoraId, lora);

        _dbContext.Loras.Remove(lora);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully handled delete LoRA request.");
        return new DeleteLoraResponse(command.LoraId);
    }
}

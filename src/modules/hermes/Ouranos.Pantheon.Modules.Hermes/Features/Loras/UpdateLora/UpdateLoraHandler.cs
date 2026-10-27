using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.UpdateLora.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Loras.UpdateLora;

public sealed class UpdateLoraHandler : IPantheonHandler<UpdateLoraInput, UpdateLoraResponse>
{
    private readonly HermesDbContext _dbContext;
    private readonly ILogger<UpdateLoraHandler> _logger;

    public UpdateLoraHandler(ILogger<UpdateLoraHandler> logger, HermesDbContext dbContext)
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<UpdateLoraResponse> Handle(
        UpdateLoraInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle update LoRA command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var lora = await _dbContext.Loras.FirstOrDefaultAsync(
            l => l.Id == command.LoraId,
            cancellationToken
        );

        Guard.Against.NotFound(command.LoraId, lora);

        await LoraValidation.ValidateIdentifierIsFreeAsync(
            _dbContext,
            command.Identifier,
            command.LoraId,
            cancellationToken
        );

        lora.Update(
            command.Name,
            command.Identifier,
            command.Weight,
            command.IsPublic,
            command.TriggerPhrase
        );

        _dbContext.Loras.Update(lora);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully handled update LoRA request.");
        return new UpdateLoraResponse(command.LoraId);
    }
}

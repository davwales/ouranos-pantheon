using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.CreateLora.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Loras.CreateLora;

public sealed class CreateLoraHandler : IPantheonHandler<CreateLoraInput, CreateLoraResponse>
{
    private readonly ILogger<CreateLoraHandler> _logger;
    private readonly HermesDbContext _dbContext;

    public CreateLoraHandler(ILogger<CreateLoraHandler> logger, HermesDbContext dbContext)
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<CreateLoraResponse> Handle(
        CreateLoraInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle create LoRA command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        await LoraValidation.ValidateIdentifierIsFreeAsync(
            _dbContext,
            command.Identifier,
            loraId: null,
            cancellationToken
        );

        var lora = Lora.Create(
            DatabaseExtensions.CreateId<Lora>(),
            command.Name,
            command.Identifier,
            command.Weight,
            command.IsPublic,
            command.TriggerPhrase
        );

        await _dbContext.Loras.AddAsync(lora, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully handled create LoRA request for LoRA '{loraId}'.", lora.Id);
        return new CreateLoraResponse(lora.Id);
    }
}

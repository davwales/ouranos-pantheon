using Ardalis.GuardClauses;
using Flagsmith;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.GetLora.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Loras.GetLora;

public sealed class GetLoraHandler : IPantheonHandler<GetLoraInput, GetLoraResponse>
{
    private readonly HermesDbContext _dbContext;
    private readonly IFlagsmithClient _flagsmith;
    private readonly ILogger<GetLoraHandler> _logger;

    public GetLoraHandler(
        ILogger<GetLoraHandler> logger,
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

    public async Task<GetLoraResponse> Handle(
        GetLoraInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get LoRA query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var lora = await _dbContext.Loras.FirstOrDefaultAsync(
            l => l.Id == query.LoraId,
            cancellationToken
        );

        Guard.Against.NotFound(query.LoraId, lora);

        if (!lora.IsPublic)
        {
            var flags = await _flagsmith.GetEnvironmentFlags();
            if (await flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode))
            {
                Guard.Against.NotFound(query.LoraId, (Lora?)null);
            }
        }

        _logger.LogDebug("Successfully handled get LoRA request.");
        return new GetLoraResponse(lora.Id, lora.Name, lora.Identifier, lora.Weight, lora.IsPublic);
    }
}

using Ardalis.GuardClauses;
using Flagsmith;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.GetAllLoras.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Filtering;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Sorting;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Loras.GetAllLoras;

public sealed class GetAllLorasHandler
    : IPantheonHandler<GetAllLorasInput, List<GetAllLorasResponse>>
{
    private static readonly FilterBuilder<Lora> FilterBuilder = new FilterBuilder<Lora>()
        .On(nameof(Lora.Name), l => l.Name, caseInsensitive: true)
        .On(nameof(Lora.Identifier), l => l.Identifier, caseInsensitive: true)
        .On(nameof(Lora.IsPublic), l => l.IsPublic);

    private static readonly SortBuilder<Lora> SortBuilder = new SortBuilder<Lora>()
        .On(nameof(Lora.Name), l => l.Name)
        .On(nameof(Lora.Identifier), l => l.Identifier)
        .Default(l => l.Name, SortDirection.Asc);

    private readonly HermesDbContext _dbContext;
    private readonly IFlagsmithClient _flagsmith;
    private readonly ILogger<GetAllLorasHandler> _logger;

    public GetAllLorasHandler(
        ILogger<GetAllLorasHandler> logger,
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

    public async Task<List<GetAllLorasResponse>> Handle(
        GetAllLorasInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get all LoRAs query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var flags = await _flagsmith.GetEnvironmentFlags();
        var isPublicMode = await flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode);

        var dbQuery = _dbContext.Loras.AsQueryable().AsNoTracking();

        if (isPublicMode)
        {
            dbQuery = dbQuery.Where(l => l.IsPublic);
        }

        var loras = await dbQuery
            .FilterBy(query.Filter, FilterBuilder)
            .SortBy(query.SortField, query.SortDirection, SortBuilder)
            .Select(l => new GetAllLorasResponse(l.Id, l.Name, l.Identifier, l.Weight, l.IsPublic))
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Successfully handled get all LoRAs request.");
        return loras;
    }
}

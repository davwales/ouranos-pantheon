using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.GetAvailableLoras.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.AvailableLoras;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Filtering;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Sorting;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Loras.GetAvailableLoras;

public sealed class GetAvailableLorasHandler
    : IPantheonHandler<GetAvailableLorasInput, List<GetAvailableLorasResponse>>
{
    private static readonly FilterBuilder<AvailableLora> FilterBuilder =
        new FilterBuilder<AvailableLora>()
            .On(nameof(AvailableLora.Identifier), l => l.Identifier, caseInsensitive: true)
            .On(nameof(AvailableLora.IsCompatible), l => l.IsCompatible);

    private static readonly SortBuilder<AvailableLora> SortBuilder =
        new SortBuilder<AvailableLora>()
            .On(nameof(AvailableLora.Identifier), l => l.Identifier)
            .On(nameof(AvailableLora.IsCompatible), l => l.IsCompatible)
            .Default(l => l.Identifier, SortDirection.Asc);

    private readonly HermesDbContext _dbContext;
    private readonly ILogger<GetAvailableLorasHandler> _logger;

    public GetAvailableLorasHandler(
        ILogger<GetAvailableLorasHandler> logger,
        HermesDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<List<GetAvailableLorasResponse>> Handle(
        GetAvailableLorasInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get available LoRAs query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var loras = await _dbContext
            .AvailableLoras.AsQueryable()
            .AsNoTracking()
            .FilterBy(query.Filter, FilterBuilder)
            .SortBy(query.SortField, query.SortDirection, SortBuilder)
            .Select(l => new GetAvailableLorasResponse(
                l.Id,
                l.Identifier,
                l.Title,
                l.CompatClass,
                l.DefaultWeight,
                l.TriggerPhrase,
                l.UsageHint,
                l.IsCompatible
            ))
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Successfully handled get available LoRAs request.");
        return loras;
    }
}

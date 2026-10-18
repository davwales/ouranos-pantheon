using Ardalis.GuardClauses;
using Flagsmith;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetToolSet.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;

namespace Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetToolSet;

public sealed class GetToolSetHandler : IPantheonHandler<GetToolSetInput, GetToolSetResponse>
{
    private readonly HermesDbContext _dbContext;
    private readonly IFlagsmithClient _flagsmith;
    private readonly ILogger<GetToolSetHandler> _logger;
    private readonly IReadOnlyList<IHermesToolSource> _toolSources;

    public GetToolSetHandler(
        ILogger<GetToolSetHandler> logger,
        HermesDbContext dbContext,
        IFlagsmithClient flagsmith,
        IEnumerable<IHermesToolSource> toolSources
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);
        Guard.Against.Null(flagsmith);
        Guard.Against.Null(toolSources);

        _logger = logger;
        _dbContext = dbContext;
        _flagsmith = flagsmith;
        _toolSources = [.. toolSources];
    }

    public async Task<GetToolSetResponse> Handle(
        GetToolSetInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get tool set query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var toolSet = await _dbContext
            .ToolSets.AsNoTracking()
            .Include(s => s.Tools)
            .FirstOrDefaultAsync(s => s.Id == query.ToolSetId, cancellationToken);

        Guard.Against.NotFound(query.ToolSetId, toolSet);

        if (!toolSet.IsPublic)
        {
            var flags = await _flagsmith.GetEnvironmentFlags();
            if (await flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode))
            {
                Guard.Against.NotFound(query.ToolSetId, (ToolSet?)null);
            }
        }

        var available = _toolSources.ListAllTools();

        _logger.LogDebug("Successfully handled get tool set request.");
        return new GetToolSetResponse(
            toolSet.Id,
            toolSet.Name,
            toolSet.Description,
            toolSet.Instructions,
            [
                .. toolSet
                    .Tools.OrderBy(t => t.Source)
                    .ThenBy(t => t.Name, StringComparer.Ordinal)
                    .Select(t => new GetToolSetToolResponse(
                        t.Source,
                        t.Name,
                        t.AdditionalDescription,
                        available.Includes(t.Source, t.Name)
                    )),
            ],
            toolSet.IsPublic
        );
    }
}

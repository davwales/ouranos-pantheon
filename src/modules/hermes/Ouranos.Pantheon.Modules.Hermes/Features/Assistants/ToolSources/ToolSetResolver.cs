using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources;

public sealed class ToolSetResolver
{
    private readonly ILogger<ToolSetResolver> _logger;
    private readonly IDbContextFactory<HermesDbContext> _dbContextFactory;
    private readonly IReadOnlyList<IHermesToolSource> _toolSources;

    public ToolSetResolver(
        ILogger<ToolSetResolver> logger,
        IDbContextFactory<HermesDbContext> dbContextFactory,
        IEnumerable<IHermesToolSource> toolSources
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContextFactory);
        Guard.Against.Null(toolSources);

        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _toolSources = [.. toolSources];
    }

    public async Task<ResolvedToolSet> ResolveAsync(
        Id<ToolSet>? toolSetId,
        HermesChatContext context,
        CancellationToken cancellationToken = default
    )
    {
        Guard.Against.Null(context);

        if (toolSetId is null)
        {
            return ResolvedToolSet.Empty;
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var toolSet = await dbContext
            .ToolSets.AsNoTracking()
            .Include(s => s.Tools)
            .FirstOrDefaultAsync(s => s.Id == toolSetId, cancellationToken);

        if (toolSet is null)
        {
            _logger.LogWarning(
                "Tool set '{toolSetId}' was not found; continuing without tools.",
                toolSetId
            );
            return ResolvedToolSet.Empty;
        }

        List<IAssistantTool<HermesChatContext>> tools =
        [
            .. _toolSources
                .SelectMany(source =>
                    source.Resolve(toolSet.Tools.Where(t => t.Source == source.Source))
                )
                .Where(t => t.IsAvailable(context)),
        ];

        return new ResolvedToolSet(tools, toolSet.Instructions);
    }
}

using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.UpdateToolSet.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;

namespace Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.UpdateToolSet;

public sealed class UpdateToolSetHandler
    : IPantheonHandler<UpdateToolSetInput, UpdateToolSetResponse>
{
    private readonly ILogger<UpdateToolSetHandler> _logger;
    private readonly HermesDbContext _dbContext;
    private readonly IReadOnlyList<IHermesToolSource> _toolSources;

    public UpdateToolSetHandler(
        ILogger<UpdateToolSetHandler> logger,
        HermesDbContext dbContext,
        IEnumerable<IHermesToolSource> toolSources
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);
        Guard.Against.Null(toolSources);

        _logger = logger;
        _dbContext = dbContext;
        _toolSources = [.. toolSources];
    }

    public async Task<UpdateToolSetResponse> Handle(
        UpdateToolSetInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle update tool set command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var toolSet = await _dbContext
            .ToolSets.Include(s => s.Tools)
            .FirstOrDefaultAsync(s => s.Id == command.ToolSetId, cancellationToken);

        Guard.Against.NotFound(command.ToolSetId, toolSet);

        var available = _toolSources.ListAllTools();
        var tools = (command.Tools ?? [])
            .Select(t =>
            {
                var isKept = toolSet.Tools.Exists(existing =>
                    existing.Source == t.Source && existing.Name == t.Name?.Trim()
                );

                Guard.Against.InvalidInput(
                    t,
                    nameof(command.Tools),
                    tool => isKept || available.Includes(tool.Source, tool.Name),
                    $"Tool '{t.Name}' is not offered by source '{t.Source}'."
                );

                return ToolSetTool.Create(
                    DatabaseExtensions.CreateId<ToolSetTool>(),
                    toolSet.Id,
                    t.Source,
                    t.Name,
                    t.AdditionalDescription
                );
            })
            .ToList();

        toolSet.Update(
            command.Name,
            command.Description,
            command.Instructions,
            tools,
            command.IsPublic
        );

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully handled update tool set request.");
        return new UpdateToolSetResponse(command.ToolSetId);
    }
}

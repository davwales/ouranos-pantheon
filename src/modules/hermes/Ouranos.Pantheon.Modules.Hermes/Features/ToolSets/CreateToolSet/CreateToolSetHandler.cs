using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.CreateToolSet.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;

namespace Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.CreateToolSet;

public sealed class CreateToolSetHandler
    : IPantheonHandler<CreateToolSetInput, CreateToolSetResponse>
{
    private readonly ILogger<CreateToolSetHandler> _logger;
    private readonly HermesDbContext _dbContext;
    private readonly IReadOnlyList<IHermesToolSource> _toolSources;

    public CreateToolSetHandler(
        ILogger<CreateToolSetHandler> logger,
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

    public async Task<CreateToolSetResponse> Handle(
        CreateToolSetInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle create tool set command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var available = _toolSources.ListAllTools();
        var toolSetId = DatabaseExtensions.CreateId<ToolSet>();

        var tools = (command.Tools ?? [])
            .Select(t =>
            {
                Guard.Against.InvalidInput(
                    t,
                    nameof(command.Tools),
                    tool => available.Includes(tool.Source, tool.Name),
                    $"Tool '{t.Name}' is not offered by source '{t.Source}'."
                );

                return ToolSetTool.Create(
                    DatabaseExtensions.CreateId<ToolSetTool>(),
                    toolSetId,
                    t.Source,
                    t.Name,
                    t.AdditionalDescription
                );
            })
            .ToList();

        var toolSet = ToolSet.Create(
            toolSetId,
            command.Name,
            command.Description,
            command.Instructions,
            tools,
            command.IsPublic
        );

        await _dbContext.ToolSets.AddAsync(toolSet, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Successfully handled create tool set request for tool set '{toolSetId}'.",
            toolSet.Id
        );
        return new CreateToolSetResponse(toolSet.Id);
    }
}

using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetAvailableTools.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;

namespace Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetAvailableTools;

public sealed class GetAvailableToolsHandler
    : IPantheonHandler<GetAvailableToolsInput, List<GetAvailableToolsResponse>>
{
    private readonly ILogger<GetAvailableToolsHandler> _logger;
    private readonly IReadOnlyList<IHermesToolSource> _toolSources;

    public GetAvailableToolsHandler(
        ILogger<GetAvailableToolsHandler> logger,
        IEnumerable<IHermesToolSource> toolSources
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(toolSources);

        _logger = logger;
        _toolSources = [.. toolSources];
    }

    public Task<List<GetAvailableToolsResponse>> Handle(
        GetAvailableToolsInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get available tools query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        List<GetAvailableToolsResponse> tools =
        [
            .. _toolSources
                .ListAllTools()
                .OrderBy(t => t.Source)
                .ThenBy(t => t.Name, StringComparer.Ordinal)
                .Select(t => new GetAvailableToolsResponse(t.Source, t.Name, t.Description)),
        ];

        _logger.LogDebug("Successfully handled get available tools request.");
        return Task.FromResult(tools);
    }
}

using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetAvailableTools;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetAvailableTools.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.ToolSets.GetAvailableTools;

public sealed class GetAvailableToolsHandlerTests
{
    private readonly ILogger<GetAvailableToolsHandler> _logger = Substitute.For<
        ILogger<GetAvailableToolsHandler>
    >();

    [Fact]
    public void Constructor_WhenToolSourcesNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<IHermesToolSource> toolSources = null!;

        // Act
        var create = () => new GetAvailableToolsHandler(_logger, toolSources);

        // Assert
        create.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_WhenToolsOffered_ShouldReturnToolsFromAllSourcesSortedByName()
    {
        // Arrange
        var handler = new GetAvailableToolsHandler(
            _logger,
            [
                CreateSource(
                    new AvailableTool(ToolSource.Native, "search", "Searches."),
                    new AvailableTool(ToolSource.Native, "Echo", "Echoes text.")
                ),
                CreateSource(new AvailableTool(ToolSource.Native, "image", "Draws.")),
            ]
        );

        // Act
        var result = await handler.Handle(new GetAvailableToolsInput(), CancellationToken.None);

        // Assert
        result.ShouldBe([
            new GetAvailableToolsResponse(ToolSource.Native, "Echo", "Echoes text."),
            new GetAvailableToolsResponse(ToolSource.Native, "image", "Draws."),
            new GetAvailableToolsResponse(ToolSource.Native, "search", "Searches."),
        ]);
    }

    [Fact]
    public async Task Handle_WhenNoSources_ShouldReturnEmptyList()
    {
        // Arrange
        var handler = new GetAvailableToolsHandler(_logger, []);

        // Act
        var result = await handler.Handle(new GetAvailableToolsInput(), CancellationToken.None);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var handler = new GetAvailableToolsHandler(_logger, []);
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () =>
            await handler.Handle(new GetAvailableToolsInput(), cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }

    private static IHermesToolSource CreateSource(params AvailableTool[] tools)
    {
        var source = Substitute.For<IHermesToolSource>();
        source.ListTools().Returns(tools);
        return source;
    }
}

using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetAvailableTools;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetAvailableTools.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.ToolSets.GetAvailableTools;

public sealed class GetAvailableToolsEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var expected = new List<GetAvailableToolsResponse>();

        _bus.InvokeAsync<List<GetAvailableToolsResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetAvailableToolsEndpoint.Handle(_bus, ct);

        // Assert
        result
            .ShouldBeOfType<Ok<List<GetAvailableToolsResponse>>>()
            .Value.ShouldBeSameAs(expected);

        await _bus.Received(1)
            .InvokeAsync<List<GetAvailableToolsResponse>>(new GetAvailableToolsInput(), ct);
    }
}

using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetAllToolSets;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetAllToolSets.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.ToolSets.GetAllToolSets;

public sealed class GetAllToolSetsEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = new GetAllToolSetsInput();
        var expected = new List<GetAllToolSetsResponse>();

        _bus.InvokeAsync<List<GetAllToolSetsResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetAllToolSetsEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<List<GetAllToolSetsResponse>>>().Value.ShouldBeSameAs(expected);
        await _bus.Received(1).InvokeAsync<List<GetAllToolSetsResponse>>(input, ct);
    }
}

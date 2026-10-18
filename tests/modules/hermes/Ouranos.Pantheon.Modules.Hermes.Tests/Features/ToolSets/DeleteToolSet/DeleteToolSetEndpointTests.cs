using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.DeleteToolSet;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.DeleteToolSet.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.ToolSets.DeleteToolSet;

public sealed class DeleteToolSetEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var toolSetId = new Id<ToolSet>(Guid.NewGuid().ToString());
        var expected = new DeleteToolSetResponse(toolSetId);

        _bus.InvokeAsync<DeleteToolSetResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await DeleteToolSetEndpoint.Handle(toolSetId, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<DeleteToolSetResponse>>().Value.ShouldBe(expected);

        await _bus.Received(1)
            .InvokeAsync<DeleteToolSetResponse>(new DeleteToolSetInput(toolSetId), ct);
    }
}

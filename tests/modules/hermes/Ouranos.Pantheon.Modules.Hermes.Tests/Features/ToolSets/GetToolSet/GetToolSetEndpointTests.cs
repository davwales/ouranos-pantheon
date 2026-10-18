using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetToolSet;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetToolSet.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.ToolSets.GetToolSet;

public sealed class GetToolSetEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var toolSetId = DatabaseExtensions.CreateId<ToolSet>();
        var expected = new GetToolSetResponse(toolSetId, "Research", null, null, [], true);

        _bus.InvokeAsync<GetToolSetResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetToolSetEndpoint.Handle(toolSetId, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<GetToolSetResponse>>().Value.ShouldBe(expected);
        await _bus.Received(1).InvokeAsync<GetToolSetResponse>(new GetToolSetInput(toolSetId), ct);
    }
}

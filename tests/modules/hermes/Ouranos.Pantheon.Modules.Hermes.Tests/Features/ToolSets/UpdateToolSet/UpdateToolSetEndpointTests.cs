using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.UpdateToolSet;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.UpdateToolSet.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.ToolSets.UpdateToolSet;

public sealed class UpdateToolSetEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var toolSetId = new Id<ToolSet>(Guid.NewGuid().ToString());

        var body = new UpdateToolSetBody(
            "Research",
            "Desc",
            "Instr",
            [new UpdateToolSetToolInput(ToolSource.Native, "echo", "Be brief.")],
            false
        );

        var expected = new UpdateToolSetResponse(toolSetId);

        _bus.InvokeAsync<UpdateToolSetResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await UpdateToolSetEndpoint.Handle(toolSetId, body, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<UpdateToolSetResponse>>().Value.ShouldBe(expected);

        await _bus.Received(1)
            .InvokeAsync<UpdateToolSetResponse>(
                Arg.Is<UpdateToolSetInput>(i =>
                    i.ToolSetId == toolSetId
                    && i.Name == body.Name
                    && i.Description == body.Description
                    && i.Instructions == body.Instructions
                    && i.Tools == body.Tools
                    && i.IsPublic == body.IsPublic
                ),
                ct
            );
    }
}

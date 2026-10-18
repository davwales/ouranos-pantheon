using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.CreateToolSet;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.CreateToolSet.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.ToolSets.CreateToolSet;

public sealed class CreateToolSetEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnCreated()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = new CreateToolSetInput(
            "Research",
            Tools: [new CreateToolSetToolInput(ToolSource.Native, "echo")]
        );
        var expected = new CreateToolSetResponse(new Id<ToolSet>(Guid.NewGuid().ToString()));

        _bus.InvokeAsync<CreateToolSetResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await CreateToolSetEndpoint.Handle(input, _bus, ct);

        // Assert
        var created = result.ShouldBeOfType<Created<CreateToolSetResponse>>();
        created.Location.ShouldBe($"/api/hermes/tool-sets/{expected.ToolSetId}");
        created.Value.ShouldBe(expected);
        await _bus.Received(1).InvokeAsync<CreateToolSetResponse>(input, ct);
    }
}

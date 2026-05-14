using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.UpdateTrait;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.UpdateTrait.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Traits.UpdateTrait;

public sealed class UpdateTraitEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var traitId = new Id<Trait>(Guid.NewGuid().ToString());
        var body = new UpdateTraitBody("Kindness", "Always be kind");
        var expected = new UpdateTraitResponse(traitId);
        var expectedInput = new UpdateTraitInput(traitId, body.Name, body.Content, body.IsPublic);

        _bus.InvokeAsync<UpdateTraitResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await UpdateTraitEndpoint.Handle(traitId, body, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<UpdateTraitResponse>>();
        await _bus.Received(1).InvokeAsync<UpdateTraitResponse>(expectedInput, ct);
    }
}

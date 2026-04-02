using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.DeleteTrait;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.DeleteTrait.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Traits.DeleteTrait;

public sealed class DeleteTraitEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var traitId = new Id<Trait>(Guid.NewGuid().ToString());
        var expected = new DeleteTraitResponse(traitId);

        _bus.InvokeAsync<DeleteTraitResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await DeleteTraitEndpoint.Handle(traitId, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<DeleteTraitResponse>>();
        await _bus.Received(1).InvokeAsync<DeleteTraitResponse>(Arg.Any<DeleteTraitInput>(), ct);
    }
}

using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.GetTrait;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.GetTrait.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Traits.GetTrait;

public sealed class GetTraitEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var traitId = new Id<Trait>(Guid.NewGuid().ToString());
        var expected = new GetTraitResponse(traitId, "Kindness", "Always be kind", true);

        _bus.InvokeAsync<GetTraitResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetTraitEndpoint.Handle(traitId, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<GetTraitResponse>>();
        await _bus.Received(1).InvokeAsync<GetTraitResponse>(Arg.Any<GetTraitInput>(), ct);
    }
}

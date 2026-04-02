using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.CreateTrait;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.CreateTrait.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Traits.CreateTrait;

public sealed class CreateTraitEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnCreated()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = new CreateTraitInput("Kindness", "Always be kind");
        var expected = new CreateTraitResponse(new Id<Trait>(Guid.NewGuid().ToString()));

        _bus.InvokeAsync<CreateTraitResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await CreateTraitEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Created<CreateTraitResponse>>();
        await _bus.Received(1).InvokeAsync<CreateTraitResponse>(input, ct);
    }
}

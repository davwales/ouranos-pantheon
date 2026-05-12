using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.GetAllTraits;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.GetAllTraits.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Traits.GetAllTraits;

public sealed class GetAllTraitsEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = new GetAllTraitsInput();
        var expected = new List<GetAllTraitsResponse>();

        _bus.InvokeAsync<List<GetAllTraitsResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetAllTraitsEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<List<GetAllTraitsResponse>>>();
        await _bus.Received(1).InvokeAsync<List<GetAllTraitsResponse>>(input, ct);
    }
}

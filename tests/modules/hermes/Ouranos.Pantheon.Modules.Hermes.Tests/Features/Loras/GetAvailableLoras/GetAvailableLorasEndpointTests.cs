using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.GetAvailableLoras;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.GetAvailableLoras.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Loras.GetAvailableLoras;

public sealed class GetAvailableLorasEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = new GetAvailableLorasInput();
        _bus.InvokeAsync<List<GetAvailableLorasResponse>>(input, ct).Returns([]);

        // Act
        var result = await GetAvailableLorasEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<List<GetAvailableLorasResponse>>>();
        await _bus.Received(1).InvokeAsync<List<GetAvailableLorasResponse>>(input, ct);
    }
}

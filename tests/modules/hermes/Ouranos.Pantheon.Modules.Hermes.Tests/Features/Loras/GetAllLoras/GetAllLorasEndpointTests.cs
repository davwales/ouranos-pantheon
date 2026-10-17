using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.GetAllLoras;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.GetAllLoras.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Loras.GetAllLoras;

public sealed class GetAllLorasEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = new GetAllLorasInput();
        var expected = new List<GetAllLorasResponse>();

        _bus.InvokeAsync<List<GetAllLorasResponse>>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetAllLorasEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<List<GetAllLorasResponse>>>();
        await _bus.Received(1).InvokeAsync<List<GetAllLorasResponse>>(input, ct);
    }
}

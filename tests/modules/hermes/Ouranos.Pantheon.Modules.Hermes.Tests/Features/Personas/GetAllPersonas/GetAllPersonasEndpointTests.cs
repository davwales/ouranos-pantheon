using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.GetAllPersonas;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.GetAllPersonas.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Personas.GetAllPersonas;

public sealed class GetAllPersonasEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = new GetAllPersonasInput();
        var expected = new List<GetAllPersonasResponse>();

        _bus.InvokeAsync<List<GetAllPersonasResponse>>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetAllPersonasEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<List<GetAllPersonasResponse>>>();
        await _bus.Received(1).InvokeAsync<List<GetAllPersonasResponse>>(input, ct);
    }
}

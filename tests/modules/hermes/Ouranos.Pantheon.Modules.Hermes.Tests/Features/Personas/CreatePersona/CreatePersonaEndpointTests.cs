using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.CreatePersona;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.CreatePersona.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Personas.CreatePersona;

public sealed class CreatePersonaEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnCreated()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = new CreatePersonaInput("Assistant", "A helpful assistant");
        var expected = new CreatePersonaResponse(new Id<Persona>(Guid.NewGuid().ToString()));

        _bus.InvokeAsync<CreatePersonaResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await CreatePersonaEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Created<CreatePersonaResponse>>();
        await _bus.Received(1).InvokeAsync<CreatePersonaResponse>(input, ct);
    }
}

using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.GetPersona;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.GetPersona.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Personas.GetPersona;

public sealed class GetPersonaEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var personaId = new Id<Persona>(Guid.NewGuid().ToString());
        var expected = new GetPersonaResponse(
            personaId,
            "Assistant",
            "A helpful assistant",
            null,
            null,
            false,
            true
        );

        _bus.InvokeAsync<GetPersonaResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetPersonaEndpoint.Handle(personaId, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<GetPersonaResponse>>();
        await _bus.Received(1).InvokeAsync<GetPersonaResponse>(Arg.Any<GetPersonaInput>(), ct);
    }
}

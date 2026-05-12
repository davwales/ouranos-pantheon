using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.DeletePersona;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.DeletePersona.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Personas.DeletePersona;

public sealed class DeletePersonaEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var personaId = new Id<Persona>(Guid.NewGuid().ToString());
        var expected = new DeletePersonaResponse(personaId);

        _bus.InvokeAsync<DeletePersonaResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await DeletePersonaEndpoint.Handle(personaId, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<DeletePersonaResponse>>();
        await _bus.Received(1)
            .InvokeAsync<DeletePersonaResponse>(Arg.Any<DeletePersonaInput>(), ct);
    }
}

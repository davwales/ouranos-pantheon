using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.UpdatePersona;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.UpdatePersona.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Personas.UpdatePersona;

public sealed class UpdatePersonaEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var personaId = new Id<Persona>(Guid.NewGuid().ToString());
        var body = new UpdatePersonaBody("Assistant", "A helpful assistant");
        var expected = new UpdatePersonaResponse(personaId);
        var expectedInput = new UpdatePersonaInput(personaId, body.Name, body.Description);

        _bus.InvokeAsync<UpdatePersonaResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await UpdatePersonaEndpoint.Handle(personaId, body, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<UpdatePersonaResponse>>();
        await _bus.Received(1).InvokeAsync<UpdatePersonaResponse>(expectedInput, ct);
    }
}

using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.DeleteLora;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.DeleteLora.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Loras.DeleteLora;

public sealed class DeleteLoraEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var loraId = new Id<Lora>(Guid.NewGuid().ToString());
        var expected = new DeleteLoraResponse(loraId);

        _bus.InvokeAsync<DeleteLoraResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await DeleteLoraEndpoint.Handle(loraId, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<DeleteLoraResponse>>();
        await _bus.Received(1).InvokeAsync<DeleteLoraResponse>(Arg.Any<DeleteLoraInput>(), ct);
    }
}

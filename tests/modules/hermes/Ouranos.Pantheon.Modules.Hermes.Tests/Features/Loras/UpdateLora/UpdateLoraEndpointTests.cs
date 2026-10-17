using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.UpdateLora;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.UpdateLora.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Loras.UpdateLora;

public sealed class UpdateLoraEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var loraId = new Id<Lora>(Guid.NewGuid().ToString());
        var body = new UpdateLoraBody("Detail", "add-detail-xl", 0.8f);
        var expected = new UpdateLoraResponse(loraId);
        var expectedInput = new UpdateLoraInput(
            loraId,
            body.Name,
            body.Identifier,
            body.Weight,
            body.IsPublic
        );

        _bus.InvokeAsync<UpdateLoraResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await UpdateLoraEndpoint.Handle(loraId, body, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<UpdateLoraResponse>>();
        await _bus.Received(1).InvokeAsync<UpdateLoraResponse>(expectedInput, ct);
    }
}

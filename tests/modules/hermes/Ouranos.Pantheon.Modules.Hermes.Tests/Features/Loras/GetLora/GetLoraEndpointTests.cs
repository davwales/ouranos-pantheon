using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.GetLora;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.GetLora.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Loras.GetLora;

public sealed class GetLoraEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var loraId = new Id<Lora>(Guid.NewGuid().ToString());
        var expected = new GetLoraResponse(
            loraId,
            "Detail",
            "add-detail-xl",
            0.8f,
            true,
            "detailed",
            IsUnavailable: false,
            IsIncompatible: false
        );

        _bus.InvokeAsync<GetLoraResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetLoraEndpoint.Handle(loraId, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<GetLoraResponse>>();
        await _bus.Received(1).InvokeAsync<GetLoraResponse>(Arg.Any<GetLoraInput>(), ct);
    }
}

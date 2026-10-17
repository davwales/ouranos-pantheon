using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.CreateLora;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.CreateLora.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Loras.CreateLora;

public sealed class CreateLoraEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnCreated()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = new CreateLoraInput("Detail", "add-detail-xl");
        var expected = new CreateLoraResponse(new Id<Lora>(Guid.NewGuid().ToString()));

        _bus.InvokeAsync<CreateLoraResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await CreateLoraEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Created<CreateLoraResponse>>();
        await _bus.Received(1).InvokeAsync<CreateLoraResponse>(input, ct);
    }
}

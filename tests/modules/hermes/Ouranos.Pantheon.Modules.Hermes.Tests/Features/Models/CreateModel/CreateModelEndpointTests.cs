using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.CreateModel;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.CreateModel.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Models.CreateModel;

public sealed class CreateModelEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnCreated()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = new CreateModelInput("GPT-4", "gpt-4", "You are helpful.");
        var expected = new CreateModelResponse(new Id<ModelConfig>(Guid.NewGuid().ToString()));

        _bus.InvokeAsync<CreateModelResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await CreateModelEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Created<CreateModelResponse>>();
        await _bus.Received(1).InvokeAsync<CreateModelResponse>(input, ct);
    }
}

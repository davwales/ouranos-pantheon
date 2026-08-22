using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.UpdateModel;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.UpdateModel.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Models.UpdateModel;

public sealed class UpdateModelEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var modelId = new Id<ModelConfig>(Guid.NewGuid().ToString());
        var body = new UpdateModelBody("GPT-4", "gpt-4", "You are helpful.");
        var expected = new UpdateModelResponse(modelId);
        var expectedInput = new UpdateModelInput(
            modelId,
            body.Name,
            body.ModelIdentifier,
            body.SystemPrompt
        );

        _bus.InvokeAsync<UpdateModelResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await UpdateModelEndpoint.Handle(modelId, body, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<UpdateModelResponse>>();
        await _bus.Received(1).InvokeAsync<UpdateModelResponse>(expectedInput, ct);
    }
}

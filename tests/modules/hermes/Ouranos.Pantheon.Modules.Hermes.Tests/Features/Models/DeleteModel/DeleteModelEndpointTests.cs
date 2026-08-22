using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.DeleteModel;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.DeleteModel.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Models.DeleteModel;

public sealed class DeleteModelEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var modelId = new Id<ModelConfig>(Guid.NewGuid().ToString());
        var expected = new DeleteModelResponse(modelId);

        _bus.InvokeAsync<DeleteModelResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await DeleteModelEndpoint.Handle(modelId, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<DeleteModelResponse>>();
        await _bus.Received(1).InvokeAsync<DeleteModelResponse>(Arg.Any<DeleteModelInput>(), ct);
    }
}

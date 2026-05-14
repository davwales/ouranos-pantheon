using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.GetModel;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.GetModel.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Shared.Extensions;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Models.GetModel;

public sealed class GetModelEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var modelId = DatabaseExtensions.CreateId<ModelConfig>();

        var expected = new GetModelResponse(
            modelId,
            "GPT-4",
            "gpt-4",
            "You are helpful.",
            null,
            null,
            null,
            null,
            false,
            true,
            false
        );

        _bus.InvokeAsync<GetModelResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetModelEndpoint.Handle(modelId, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<GetModelResponse>>();
        await _bus.Received(1).InvokeAsync<GetModelResponse>(Arg.Any<GetModelInput>(), ct);
    }
}

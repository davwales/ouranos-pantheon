using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.GetAllModels;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.GetAllModels.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Models.GetAllModels;

public sealed class GetAllModelsEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = new GetAllModelsInput();
        var expected = new List<GetAllModelsResponse>();

        _bus.InvokeAsync<List<GetAllModelsResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetAllModelsEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<List<GetAllModelsResponse>>>();
        await _bus.Received(1).InvokeAsync<List<GetAllModelsResponse>>(input, ct);
    }
}

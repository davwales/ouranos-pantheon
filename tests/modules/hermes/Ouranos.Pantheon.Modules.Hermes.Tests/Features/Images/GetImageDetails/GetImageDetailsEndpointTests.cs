using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.Images.GetImageDetails;
using Ouranos.Pantheon.Modules.Hermes.Features.Images.GetImageDetails.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Images;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Images.GetImageDetails;

public sealed class GetImageDetailsEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnTheDetails()
    {
        // Arrange
        var imageId = new Id<GeneratedImage>(Guid.NewGuid().ToString());
        var details = new GetImageDetailsResponse(
            imageId,
            64,
            64,
            "A fox",
            "flux",
            null,
            null,
            DateTimeOffset.UtcNow
        );

        _bus.InvokeAsync<GetImageDetailsResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(details));

        // Act
        var result = await GetImageDetailsEndpoint.Handle(imageId, _bus, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Ok<GetImageDetailsResponse>>().Value.ShouldBe(details);
        await _bus.Received(1)
            .InvokeAsync<GetImageDetailsResponse>(
                Arg.Is<GetImageDetailsInput>(i => i.ImageId == imageId),
                Arg.Any<CancellationToken>()
            );
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.Images.GetImage;
using Ouranos.Pantheon.Modules.Hermes.Features.Images.GetImage.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Images;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Images.GetImage;

public sealed class GetImageEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnTheImageFileWithAPrivateCache()
    {
        // Arrange
        var imageId = new Id<GeneratedImage>(Guid.NewGuid().ToString());
        var httpContext = new DefaultHttpContext();

        _bus.InvokeAsync<GetImageResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new GetImageResponse([1, 2], "image/png")));

        // Act
        var result = await GetImageEndpoint.Handle(
            imageId,
            httpContext.Response,
            _bus,
            CancellationToken.None
        );

        // Assert
        var file = result.ShouldBeOfType<FileContentHttpResult>();
        file.FileContents.ToArray().ShouldBe([1, 2]);
        file.ContentType.ShouldBe("image/png");
        httpContext.Response.Headers.CacheControl.ToString().ShouldBe("private, max-age=86400");

        await _bus.Received(1)
            .InvokeAsync<GetImageResponse>(
                Arg.Is<GetImageInput>(i => i.ImageId == imageId),
                Arg.Any<CancellationToken>()
            );
    }
}

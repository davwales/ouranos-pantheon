using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.GetFolder;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.GetFolder.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Folders.GetFolder;

public sealed class GetFolderEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var folderId = new Id<Folder>(Guid.NewGuid().ToString());
        var expected = new GetFolderResponse(
            folderId,
            "Test Folder",
            true,
            null,
            0,
            0,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow
        );

        _bus.InvokeAsync<GetFolderResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetFolderEndpoint.Handle(folderId, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<GetFolderResponse>>();
        await _bus.Received(1).InvokeAsync<GetFolderResponse>(Arg.Any<GetFolderInput>(), ct);
    }

    [Fact]
    public async Task Handle_WhenCalled_ShouldPassCorrectFolderId()
    {
        // Arrange
        var ct = CancellationToken.None;
        var folderId = new Id<Folder>(Guid.NewGuid().ToString());
        var expected = new GetFolderResponse(
            folderId,
            "Test Folder",
            true,
            null,
            0,
            0,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow
        );

        _bus.InvokeAsync<GetFolderResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        await GetFolderEndpoint.Handle(folderId, _bus, ct);

        // Assert
        await _bus.Received(1)
            .InvokeAsync<GetFolderResponse>(
                Arg.Is<GetFolderInput>(i => i.FolderId == folderId),
                ct
            );
    }
}

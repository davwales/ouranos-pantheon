using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.DeleteFolder;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.DeleteFolder.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Folders.DeleteFolder;

public sealed class DeleteFolderEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var folderId = new Id<Folder>(Guid.NewGuid().ToString());
        var expected = new IdResponse<Folder>(folderId);

        _bus.InvokeAsync<IdResponse<Folder>>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await DeleteFolderEndpoint.Handle(folderId, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<IdResponse<Folder>>>();
        await _bus.Received(1).InvokeAsync<IdResponse<Folder>>(Arg.Any<DeleteFolderInput>(), ct);
    }

    [Fact]
    public async Task Handle_WhenCalled_ShouldPassCorrectFolderId()
    {
        // Arrange
        var ct = CancellationToken.None;
        var folderId = new Id<Folder>(Guid.NewGuid().ToString());
        var expected = new IdResponse<Folder>(folderId);

        _bus.InvokeAsync<IdResponse<Folder>>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        await DeleteFolderEndpoint.Handle(folderId, _bus, ct);

        // Assert
        await _bus.Received(1)
            .InvokeAsync<IdResponse<Folder>>(
                Arg.Is<DeleteFolderInput>(i => i.FolderId == folderId),
                ct
            );
    }
}

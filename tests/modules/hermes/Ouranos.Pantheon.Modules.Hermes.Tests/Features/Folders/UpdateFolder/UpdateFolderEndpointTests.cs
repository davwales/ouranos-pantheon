using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.UpdateFolder;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.UpdateFolder.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Folders.UpdateFolder;

public sealed class UpdateFolderEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var folderId = new Id<Folder>(Guid.NewGuid().ToString());
        var body = new UpdateFolderBody("Updated Folder", true, null);
        var expected = new IdResponse<Folder>(folderId);

        _bus.InvokeAsync<IdResponse<Folder>>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await UpdateFolderEndpoint.Handle(folderId, body, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<IdResponse<Folder>>>();
        await _bus.Received(1)
            .InvokeAsync<IdResponse<Folder>>(
                Arg.Is<UpdateFolderInput>(i =>
                    i.FolderId == folderId
                    && i.Name == body.Name
                    && i.IsPublic == body.IsPublic
                    && i.ParentFolderId == body.ParentFolderId
                ),
                ct
            );
    }

    [Fact]
    public async Task Handle_WhenCalledWithParentFolderId_ShouldPassCorrectInput()
    {
        // Arrange
        var ct = CancellationToken.None;
        var folderId = new Id<Folder>(Guid.NewGuid().ToString());
        var parentFolderId = new Id<Folder>(Guid.NewGuid().ToString());
        var body = new UpdateFolderBody("Moved Folder", false, parentFolderId);
        var expected = new IdResponse<Folder>(folderId);

        _bus.InvokeAsync<IdResponse<Folder>>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await UpdateFolderEndpoint.Handle(folderId, body, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<IdResponse<Folder>>>();
        await _bus.Received(1)
            .InvokeAsync<IdResponse<Folder>>(
                Arg.Is<UpdateFolderInput>(i =>
                    i.FolderId == folderId
                    && i.Name == body.Name
                    && i.IsPublic == body.IsPublic
                    && i.ParentFolderId == parentFolderId
                ),
                ct
            );
    }
}

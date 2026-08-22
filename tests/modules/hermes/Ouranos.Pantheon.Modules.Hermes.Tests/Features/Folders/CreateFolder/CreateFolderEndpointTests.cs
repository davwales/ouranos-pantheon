using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.CreateFolder;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.CreateFolder.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Folders.CreateFolder;

public sealed class CreateFolderEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnCreated()
    {
        // Arrange
        var ct = CancellationToken.None;
        var folderId = new Id<Folder>(Guid.NewGuid().ToString());
        var input = new CreateFolderInput("New Folder");
        var expected = new CreateFolderResponse(folderId);

        _bus.InvokeAsync<CreateFolderResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await CreateFolderEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Created<CreateFolderResponse>>();
        await _bus.Received(1).InvokeAsync<CreateFolderResponse>(input, ct);
    }

    [Fact]
    public async Task Handle_WhenCalledWithParentFolderId_ShouldReturnCreated()
    {
        // Arrange
        var ct = CancellationToken.None;
        var parentFolderId = new Id<Folder>(Guid.NewGuid().ToString());
        var folderId = new Id<Folder>(Guid.NewGuid().ToString());
        var input = new CreateFolderInput(
            "Sub Folder",
            IsPublic: false,
            ParentFolderId: parentFolderId
        );
        var expected = new CreateFolderResponse(folderId);

        _bus.InvokeAsync<CreateFolderResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await CreateFolderEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Created<CreateFolderResponse>>();
        await _bus.Received(1).InvokeAsync<CreateFolderResponse>(input, ct);
    }
}

using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.GetAllFolders;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.GetAllFolders.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Folders.GetAllFolders;

public sealed class GetAllFoldersEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = new GetAllFoldersInput();
        var expected = new List<FolderSummary>();

        _bus.InvokeAsync<List<FolderSummary>>(
                Arg.Any<GetAllFoldersInput>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetAllFoldersEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<List<FolderSummary>>>();
        await _bus.Received(1).InvokeAsync<List<FolderSummary>>(Arg.Any<GetAllFoldersInput>(), ct);
    }

    [Fact]
    public async Task Handle_WhenParentFolderIdProvided_ShouldPassInputToBus()
    {
        // Arrange
        var ct = CancellationToken.None;
        var parentFolderId = new Id<Folder>(Guid.NewGuid().ToString());
        var input = new GetAllFoldersInput(ParentFolderId: parentFolderId);
        var expected = new List<FolderSummary>();

        _bus.InvokeAsync<List<FolderSummary>>(
                Arg.Any<GetAllFoldersInput>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetAllFoldersEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<List<FolderSummary>>>();
        await _bus.Received(1)
            .InvokeAsync<List<FolderSummary>>(
                Arg.Is<GetAllFoldersInput>(i => i.ParentFolderId == parentFolderId),
                ct
            );
    }
}

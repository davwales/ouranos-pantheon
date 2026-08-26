using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.DeleteFolder.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Folders.DeleteFolder;

public sealed class DeleteFolderHandler(
    ILogger<DeleteFolderHandler> logger,
    HermesDbContext dbContext
) : IPantheonHandler<DeleteFolderInput, IdResponse<Folder>>
{
    private readonly HermesDbContext _dbContext = Guard.Against.Null(dbContext);
    private readonly ILogger<DeleteFolderHandler> _logger = Guard.Against.Null(logger);

    public async Task<IdResponse<Folder>> Handle(
        DeleteFolderInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle delete folder command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var folder = await _dbContext.Folders.FirstOrDefaultAsync(
            f => f.Id == command.FolderId,
            cancellationToken
        );

        Guard.Against.NotFound(command.FolderId, folder);

        _logger.LogWarning(
            "Deleting folder {FolderId} '{FolderName}' and cascading to all descendants",
            folder.Id,
            folder.Name
        );

        _dbContext.Folders.Remove(folder);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully handled delete folder request.");
        return new IdResponse<Folder>(command.FolderId);
    }
}

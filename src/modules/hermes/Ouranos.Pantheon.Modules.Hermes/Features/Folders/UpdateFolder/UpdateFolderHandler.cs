using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.UpdateFolder.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Folders.UpdateFolder;

public sealed class UpdateFolderHandler(
    ILogger<UpdateFolderHandler> logger,
    HermesDbContext dbContext
) : IPantheonHandler<UpdateFolderInput, IdResponse<Folder>>
{
    private readonly HermesDbContext _dbContext = Guard.Against.Null(dbContext);
    private readonly ILogger<UpdateFolderHandler> _logger = Guard.Against.Null(logger);

    public async Task<IdResponse<Folder>> Handle(
        UpdateFolderInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle update folder command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var folder = await _dbContext.Folders.FirstOrDefaultAsync(
            f => f.Id == command.FolderId,
            cancellationToken
        );

        Guard.Against.NotFound(command.FolderId, folder);

        if (command.ParentFolderId is not null)
        {
            var parentFolder = await _dbContext.Folders.FirstOrDefaultAsync(
                f => f.Id == command.ParentFolderId.Value,
                cancellationToken
            );
            Guard.Against.NotFound(command.ParentFolderId.Value, parentFolder);

            if (command.ParentFolderId != folder.ParentFolderId)
            {
                await ValidateParentFolderChange(
                    command.FolderId,
                    command.ParentFolderId.Value,
                    cancellationToken
                );
            }
        }

        folder.Update(command.Name, command.IsPublic, command.ParentFolderId);

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Successfully handled update folder request for folder '{folderId}'.",
            folder.Id
        );
        return new IdResponse<Folder>(folder.Id);
    }

    private async Task ValidateParentFolderChange(
        Id<Folder> folderId,
        Id<Folder> parentFolderId,
        CancellationToken cancellationToken
    )
    {
        if (parentFolderId == folderId)
        {
            throw new BadRequestException("A folder cannot be moved into itself.");
        }

        var hasLoop = await FolderTreeValidation.CheckForLoopAsync(
            parentFolderId,
            _dbContext,
            targetFolderId: folderId,
            cancellationToken: cancellationToken
        );

        if (hasLoop)
        {
            throw new BadRequestException("A folder cannot be moved into one of its subfolders.");
        }

        var parentDepth = await FolderTreeValidation.CalculateDepthAsync(
            parentFolderId,
            _dbContext,
            cancellationToken
        );
        var subtreeHeight = await FolderTreeValidation.CalculateSubtreeHeightAsync(
            folderId,
            _dbContext,
            cancellationToken
        );

        if (parentDepth + subtreeHeight > Folder.MaxDepth)
        {
            throw new BadRequestException(
                $"Moving this folder here would nest folders more than {Folder.MaxDepth} levels deep."
            );
        }
    }
}

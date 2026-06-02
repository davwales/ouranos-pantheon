using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.UpdateFolder.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;

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

            await ValidateParentFolderChange(
                command.FolderId,
                command.ParentFolderId.Value,
                cancellationToken
            );
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
        Guard.Against.InvalidInput(
            parentFolderId,
            nameof(parentFolderId),
            id => id != folderId,
            "A folder cannot be its own parent."
        );

        var hasLoop = await FolderTreeValidation.CheckForLoopAsync(
            parentFolderId,
            _dbContext,
            targetFolderId: folderId,
            cancellationToken: cancellationToken
        );

        if (hasLoop)
        {
            throw new ArgumentException(
                "Moving this folder would create an infinite loop.",
                nameof(parentFolderId)
            );
        }

        var depth = await FolderTreeValidation.CalculateDepthAsync(
            parentFolderId,
            _dbContext,
            cancellationToken
        );

        if (depth >= Folder.MaxDepth)
        {
            throw new ArgumentException(
                $"Folder nesting depth cannot exceed {Folder.MaxDepth} levels.",
                nameof(parentFolderId)
            );
        }
    }
}

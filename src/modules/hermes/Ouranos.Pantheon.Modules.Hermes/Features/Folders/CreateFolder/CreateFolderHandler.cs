using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.CreateFolder.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Extensions;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Folders.CreateFolder;

public sealed class CreateFolderHandler(
    ILogger<CreateFolderHandler> logger,
    HermesDbContext dbContext
) : IPantheonHandler<CreateFolderInput, CreateFolderResponse>
{
    private readonly HermesDbContext _dbContext = Guard.Against.Null(dbContext);
    private readonly ILogger<CreateFolderHandler> _logger = Guard.Against.Null(logger);

    public async Task<CreateFolderResponse> Handle(
        CreateFolderInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle create folder command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        Folder? parentFolder = null;

        if (command.ParentFolderId is not null)
        {
            parentFolder = await _dbContext.Folders.FirstOrDefaultAsync(
                f => f.Id == command.ParentFolderId,
                cancellationToken
            );

            Guard.Against.Null(parentFolder);

            var depth = await FolderTreeValidation.CalculateDepthAsync(
                command.ParentFolderId.Value,
                _dbContext,
                cancellationToken
            );

            if (depth >= Folder.MaxDepth)
            {
                throw new ArgumentException(
                    $"Folder nesting depth cannot exceed {Folder.MaxDepth} levels.",
                    nameof(command.ParentFolderId)
                );
            }
        }

        var folderId = DatabaseExtensions.CreateId<Folder>();

        var folder = Folder.Create(
            folderId,
            command.Name,
            command.IsPublic,
            command.ParentFolderId
        );

        await _dbContext.Folders.AddAsync(folder, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Successfully handled create folder request for folder '{folderId}'.",
            folder.Id
        );
        return new CreateFolderResponse(folder.Id);
    }
}

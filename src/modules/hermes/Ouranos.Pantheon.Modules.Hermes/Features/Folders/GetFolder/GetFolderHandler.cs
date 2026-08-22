using Ardalis.GuardClauses;
using Flagsmith;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.GetFolder.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Folders.GetFolder;

public sealed class GetFolderHandler(
    ILogger<GetFolderHandler> logger,
    HermesDbContext dbContext,
    IFlagsmithClient flagsmith
) : IPantheonHandler<GetFolderInput, GetFolderResponse>
{
    private readonly HermesDbContext _dbContext = Guard.Against.Null(dbContext);
    private readonly IFlagsmithClient _flagsmith = Guard.Against.Null(flagsmith);
    private readonly ILogger<GetFolderHandler> _logger = Guard.Against.Null(logger);

    public async Task<GetFolderResponse> Handle(
        GetFolderInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get folder query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var folder = await _dbContext
            .Folders.Include(f => f.Conversations)
            .Include(f => f.ChildFolders)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == query.FolderId, cancellationToken);

        Guard.Against.NotFound(query.FolderId, folder);

        var flags = await flagsmith.GetEnvironmentFlags();
        var isPublicMode = await flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode);

        if (isPublicMode && !folder.IsPublic)
        {
            Guard.Against.NotFound(folder.Id, (Folder?)null);
        }

        _logger.LogDebug("Successfully handled get folder request.");

        return new GetFolderResponse(
            folder.Id,
            folder.Name,
            folder.IsPublic,
            folder.ParentFolderId,
            folder.Conversations.Count,
            folder.ChildFolders.Count,
            folder.CreatedAt,
            folder.UpdatedAt
        );
    }
}

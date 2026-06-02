using Ardalis.GuardClauses;
using Flagsmith;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.GetAllFolders.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Sorting;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Folders.GetAllFolders;

public sealed class GetAllFoldersHandler(
    ILogger<GetAllFoldersHandler> logger,
    HermesDbContext dbContext,
    IFlagsmithClient flagsmith
) : IPantheonHandler<GetAllFoldersInput, List<FolderSummary>>
{
    private static readonly FilterBuilder<Folder> FilterBuilder = new FilterBuilder<Folder>().On(
        nameof(Folder.Name),
        f => f.Name,
        caseInsensitive: true
    );

    private static readonly SortBuilder<Folder> SortBuilder = new SortBuilder<Folder>()
        .On(nameof(Folder.Name), f => f.Name)
        .On(nameof(Folder.UpdatedAt), f => f.UpdatedAt)
        .Default(f => f.UpdatedAt);

    private readonly HermesDbContext _dbContext = Guard.Against.Null(dbContext);
    private readonly IFlagsmithClient _flagsmith = Guard.Against.Null(flagsmith);
    private readonly ILogger<GetAllFoldersHandler> _logger = Guard.Against.Null(logger);

    public async Task<List<FolderSummary>> Handle(
        GetAllFoldersInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get all folders query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var flags = await _flagsmith.GetEnvironmentFlags();
        var isPublicMode = await flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode);

        var dbQuery = _dbContext.Folders.AsQueryable().AsNoTracking();

        if (query.ParentFolderId is not null)
        {
            dbQuery = dbQuery.Where(f => f.ParentFolderId == query.ParentFolderId);
        }

        if (isPublicMode)
        {
            dbQuery = dbQuery.Where(f => f.IsPublic);
        }

        var folders = await dbQuery
            .FilterBy(query.Filter, FilterBuilder)
            .SortBy(query.SortField, query.SortDirection, SortBuilder)
            .Select(f => new FolderSummary(
                f.Id,
                f.Name,
                f.IsPublic,
                f.ParentFolderId,
                f.Conversations.Count,
                f.ChildFolders.Count,
                f.CreatedAt,
                f.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Successfully handled get all folders request.");
        return folders;
    }
}

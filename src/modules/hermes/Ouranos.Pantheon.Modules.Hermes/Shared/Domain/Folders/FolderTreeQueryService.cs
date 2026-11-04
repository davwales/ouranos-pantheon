using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Querying;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;

public sealed class FolderTreeQueryService(HermesDbContext dbContext) : IFolderTreeQueryService
{
    private readonly HermesDbContext _dbContext = Guard.Against.Null(dbContext);

    public async Task<int> CalculateSubtreeHeightAsync(
        Id<Folder> folderId,
        CancellationToken cancellationToken = default
    )
    {
        // The height bound also terminates the recursion if the table ever contains a cycle.
        var command = RawSqlCommand
            .FromSql(
                """
                WITH RECURSIVE subtree AS (
                    SELECT f.id, 1 AS height
                    FROM hermes.folders f
                    WHERE f.id = @folderId
                    UNION ALL
                    SELECT child.id, subtree.height + 1
                    FROM hermes.folders child
                    JOIN subtree ON child.parent_folder_id = subtree.id
                    WHERE subtree.height <= @maxDepth
                )
                SELECT COALESCE(MAX(height), 1) AS "Value"
                FROM subtree
                """
            )
            .WithId("@folderId", folderId)
            .WithInt("@maxDepth", Folder.MaxDepth);

        var heights = await _dbContext
            .Database.SqlQueryRaw<int>(command.Sql, command.GetParameters())
            .ToListAsync(cancellationToken);

        return heights.Single();
    }
}

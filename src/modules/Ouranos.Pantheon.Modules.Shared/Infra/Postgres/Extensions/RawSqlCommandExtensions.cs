using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Querying;

namespace Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Extensions;

/// <summary>
///     Extension methods for executing <see cref="RawSqlCommand" /> queries
///     against a <see cref="DatabaseFacade" />.
/// </summary>
public static class RawSqlCommandExtensions
{
    /// <summary>
    ///     Executes a <see cref="RawSqlCommand" /> and maps the results to <typeparamref name="T" />.
    /// </summary>
    public static async Task<List<T>> ExecuteQueryAsync<T>(
        this DatabaseFacade database,
        RawSqlCommand command,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        return await database
            .SqlQueryRaw<T>(command.Sql, command.GetParameters())
            .ToListAsync(cancellationToken);
    }
}

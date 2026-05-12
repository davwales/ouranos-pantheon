using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Querying;

/// <summary>
///     Helper for constructing raw SQL queries with strongly-typed parameters.
/// </summary>
public sealed class RawSqlCommand
{
    private readonly List<NpgsqlParameter> _parameters = [];

    /// <summary>
    ///     The SQL query text with <c>@param</c> placeholders.
    /// </summary>
    public string Sql { get; }

    private RawSqlCommand(string sql)
    {
        Sql = sql;
    }

    /// <summary>
    ///     Creates a new <see cref="RawSqlCommand" /> from the given SQL template.
    /// </summary>
    public static RawSqlCommand FromSql(string sql)
    {
        return new RawSqlCommand(sql);
    }

    /// <summary>
    ///     Adds a <see cref="Id{T}" /> parameter, converting it to a PostgreSQL UUID.
    /// </summary>
    public RawSqlCommand WithId<T>(string name, Id<T> id)
    {
        _parameters.Add(
            new NpgsqlParameter(name, NpgsqlDbType.Uuid) { Value = Guid.Parse(id.Value) }
        );
        return this;
    }

    /// <summary>
    ///     Adds a list of <see cref="Id{T}" /> as a PostgreSQL UUID array parameter
    ///     for use with <c>= ANY(@param)</c> clauses.
    /// </summary>
    public RawSqlCommand WithIds<T>(string name, IEnumerable<Id<T>> ids)
    {
        var uuids = ids.Select(id => Guid.Parse(id.Value)).ToArray();
        _parameters.Add(
            new NpgsqlParameter(name, NpgsqlDbType.Array | NpgsqlDbType.Uuid) { Value = uuids }
        );
        return this;
    }

    /// <summary>
    ///     Adds a nullable <see cref="DateTimeOffset" /> parameter.
    /// </summary>
    public RawSqlCommand WithDateTimeOffset(string name, DateTimeOffset? value)
    {
        _parameters.Add(new NpgsqlParameter(name, value));
        return this;
    }

    /// <summary>
    ///     Adds a <see cref="DateTimeOffset" /> parameter.
    /// </summary>
    public RawSqlCommand WithDateTimeOffset(string name, DateTimeOffset value)
    {
        _parameters.Add(new NpgsqlParameter(name, NpgsqlDbType.TimestampTz) { Value = value });
        return this;
    }

    /// <summary>
    ///     Adds a <see cref="string" /> parameter.
    /// </summary>
    public RawSqlCommand WithString(string name, string value)
    {
        _parameters.Add(new NpgsqlParameter(name, NpgsqlDbType.Text) { Value = value });
        return this;
    }

    /// <summary>
    ///     Adds a <see cref="decimal" /> parameter.
    /// </summary>
    public RawSqlCommand WithDecimal(string name, decimal value)
    {
        _parameters.Add(new NpgsqlParameter(name, NpgsqlDbType.Numeric) { Value = value });
        return this;
    }

    /// <summary>
    ///     Adds a <see cref="int" /> parameter.
    /// </summary>
    public RawSqlCommand WithInt(string name, int value)
    {
        _parameters.Add(new NpgsqlParameter(name, NpgsqlDbType.Integer) { Value = value });
        return this;
    }

    /// <summary>
    ///     Adds a generic <see cref="NpgsqlParameter" /> for advanced use cases.
    /// </summary>
    public RawSqlCommand WithParameter(NpgsqlParameter parameter)
    {
        _parameters.Add(parameter);
        return this;
    }

    /// <summary>
    ///     Returns the ordered array of parameters for passing to
    ///     <see cref="RelationalDatabaseFacadeExtensions.SqlQueryRaw{T}" />.
    /// </summary>
    public object[] GetParameters()
    {
        return _parameters.Cast<object>().ToArray();
    }
}

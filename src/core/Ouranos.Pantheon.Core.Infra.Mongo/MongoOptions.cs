namespace Ouranos.Pantheon.Core.Infra.Mongo;

public sealed record MongoOptions(
    string ConnectionString,
    Dictionary<string, string> AssemblyDatabases,
    Dictionary<string, string> TypeDatabases
)
{
    public const string SectionName = "Ouranos:Mongo";

    public MongoOptions() : this(string.Empty, [], [])
    {
    }
}
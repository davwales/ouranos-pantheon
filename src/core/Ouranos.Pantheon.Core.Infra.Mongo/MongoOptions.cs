namespace Ouranos.Pantheon.Core.Infra.Mongo;

public sealed record MongoOptions(
    string ConnectionString,
    Dictionary<string, string> AssemblyDatabases,
    Dictionary<string, string> TypeDatabases
)
{
    public const string SectionName = "Mongo";
    
    public MongoOptions() : this("", [], [])
    {}
}

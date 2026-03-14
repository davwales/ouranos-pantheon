namespace Ouranos.Pantheon.Core.Infra.Postgres;

public sealed record PostgresOptions(
    string Host,
    int Port,
    string Database,
    string Username,
    string Password,
    string? SearchPath,
    bool IncludeErrorDetail,
    int CommandTimeout,
    int MaxRetries,
    uint MaxRetryDelaySeconds,
    bool EnableSensitiveDataLogging
)
{
    public const string SectionName = "Ouranos:Postgres";

    public PostgresOptions() : this(
        string.Empty,
        5432,
        string.Empty,
        string.Empty,
        string.Empty,
        null,
        false,
        30,
        3,
        5,
        false
    )
    {
    }

    public string GetConnectionString()
    {
        var connectionString = $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password};";

        if (!string.IsNullOrWhiteSpace(SearchPath))
        {
            connectionString += $"Search Path={SearchPath};";
        }

        if (IncludeErrorDetail)
        {
            connectionString += "Include Error Detail=true;";
        }

        connectionString += $"Command Timeout={CommandTimeout};";

        return connectionString;
    }
}
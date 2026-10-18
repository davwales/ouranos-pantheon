namespace Ouranos.Pantheon.Modules.Shared.Infra.Flagsmith;

public sealed record FlagsmithOptions(string ApiUrl, string EnvironmentKey)
{
    public const string SectionName = "Ouranos:Flagsmith";

    public FlagsmithOptions()
        : this(ApiUrl: string.Empty, EnvironmentKey: string.Empty) { }

    public int CacheDurationMinutes { get; init; } = 1;

    public double RequestTimeoutSeconds { get; init; } = 2;

    public int Retries { get; init; } = 1;

    public IReadOnlyDictionary<string, bool> DefaultFlags { get; init; } =
        new Dictionary<string, bool>();
}

namespace Ouranos.Pantheon.Modules.Shared.Infra.Flagsmith;

public sealed record FlagsmithOptions(string ApiUrl, string EnvironmentKey)
{
    public const string SectionName = "Ouranos:Flagsmith";

    public FlagsmithOptions() : this(
        ApiUrl: string.Empty,
        EnvironmentKey: string.Empty
    )
    {
    }
}

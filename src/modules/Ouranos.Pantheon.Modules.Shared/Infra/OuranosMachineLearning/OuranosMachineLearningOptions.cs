namespace Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning;

public sealed record OuranosMachineLearningOptions(string ConnectionString, string ApiKey)
{
    public const string SectionName = "Ouranos:OuranosMl";

    public OuranosMachineLearningOptions() : this(
        ConnectionString: string.Empty,
        ApiKey: "no-key"
    )
    {
    }
}

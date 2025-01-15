namespace Ouranos.Pantheon.Service.Hermes.Infra.OuranosMl;

public sealed record OuranosMachineLearningOptions(
    string SystemPrompt
)
{
    public const string SectionName = "Ouranos:Hermes:OuranosMl";

    public OuranosMachineLearningOptions() : this(string.Empty)
    {
    }
}
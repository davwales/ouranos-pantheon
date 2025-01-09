namespace Ouranos.Pantheon.Core.Infra.OuranosMl;

public sealed record OuranosMlOptions(
    string ConnectionString
)
{
    public const string SectionName = "Ouranos:OuranosMl";

    public OuranosMlOptions() : this(string.Empty)
    {
    }
}
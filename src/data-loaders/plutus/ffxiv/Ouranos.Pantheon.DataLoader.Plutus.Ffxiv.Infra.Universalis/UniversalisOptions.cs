namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Infra.Universalis;

public sealed record UniversalisOptions(
    IReadOnlyCollection<int> Worlds
)
{
    public const string SectionName = "Ouranos:Universalis";

    public UniversalisOptions() : this([])
    {
    }
}
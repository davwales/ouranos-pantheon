using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Shared;

namespace Ouranos.Pantheon.Modules.Plutus.Shared;

public sealed record PlutusOptions(
    DataLoadersOptions DataLoaders
)
{
    public const string SectionName = "Ouranos:Plutus";

    public PlutusOptions() : this(DataLoaders: new DataLoadersOptions())
    {
    }
}

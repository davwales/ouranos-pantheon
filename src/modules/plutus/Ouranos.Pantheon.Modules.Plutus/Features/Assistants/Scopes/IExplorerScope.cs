namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Scopes;

/// <summary>
/// <see cref="View"/> is set only while the user is on the explorer, where the table can change.
/// </summary>
public interface IExplorerScope : IMarketScope
{
    ExplorerView? View { get; }
}

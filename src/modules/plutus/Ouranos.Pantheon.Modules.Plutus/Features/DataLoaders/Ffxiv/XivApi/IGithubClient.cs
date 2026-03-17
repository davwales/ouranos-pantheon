namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.XivApi;

public interface IGithubClient
{
    Task<List<ItemResponse>> GetItems(
        CancellationToken cancellationToken = default
    );
}

using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Infra.XivApi.Models;

namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Infra.XivApi;

public interface IGithubClient
{
    Task<List<ItemResponse>> GetItems(
        CancellationToken cancellationToken = default
    );
}
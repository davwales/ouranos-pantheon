using Ouranos.Pantheon.Plutus.DataLoader.Ffxiv.Infra.XivApi.Models;

namespace Ouranos.Pantheon.Plutus.DataLoader.Ffxiv.Infra.XivApi;

public interface IGithubClient
{
    Task<List<ItemResponse>> GetItems(
        CancellationToken cancellationToken = default
    );
}
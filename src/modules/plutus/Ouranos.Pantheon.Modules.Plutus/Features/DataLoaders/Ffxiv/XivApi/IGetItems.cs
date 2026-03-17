namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.XivApi;

public interface IGetItems
{
    Task<List<ItemDto>> GetItemsAsync(CancellationToken cancellationToken = default);
}

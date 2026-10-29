namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.XivApi;

public interface IGetItems
{
    Task<ItemDto?> GetItemAsync(
        string symbolCode,
        bool isHighQuality,
        CancellationToken cancellationToken = default
    );
}

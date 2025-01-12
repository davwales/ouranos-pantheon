using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Application.Dtos;

namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Application.Interfaces.Items;

public interface IGetItems
{
    Task<List<ItemDto>> GetItemsAsync(CancellationToken cancellationToken = default);
}
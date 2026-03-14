using Ouranos.Pantheon.Plutus.DataLoader.Ffxiv.Application.Dtos;

namespace Ouranos.Pantheon.Plutus.DataLoader.Ffxiv.Application.Interfaces.Items;

public interface IGetItems
{
    Task<List<ItemDto>> GetItemsAsync(CancellationToken cancellationToken = default);
}
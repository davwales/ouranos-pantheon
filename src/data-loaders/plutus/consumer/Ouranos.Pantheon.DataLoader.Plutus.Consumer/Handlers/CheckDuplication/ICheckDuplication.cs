namespace Ouranos.Pantheon.DataLoader.Plutus.Consumer.Handlers.CheckDuplication;

public interface ICheckDuplication
{
    Task<bool> CheckDuplicationAsync(
        Guid messageId,
        CancellationToken cancellationToken = default
    );
}
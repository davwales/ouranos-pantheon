namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Application.Interfaces.Subscriptions;

public interface ISetupSubscriptions
{
    Task Setup(CancellationToken cancellationToken = default);
}
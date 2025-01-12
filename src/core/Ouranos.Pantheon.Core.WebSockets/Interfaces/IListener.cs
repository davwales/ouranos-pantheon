namespace Ouranos.Pantheon.Core.WebSockets.Interfaces;

public interface IListener
{
    Task OnConnectedAsync(
        CancellationToken cancellationToken
    );

    Task HandleMessageAsync(
        byte[] message,
        CancellationToken cancellationToken
    );
}
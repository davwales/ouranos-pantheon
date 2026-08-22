namespace Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.Serializers;

public interface IMessageSerializer
{
    byte[] Serialize<T>(T message);

    T Deserialize<T>(byte[] data);
}

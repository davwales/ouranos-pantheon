namespace Ouranos.Pantheon.Modules.Shared.WebSockets.Serializers;

public interface IMessageSerializer
{
    byte[] Serialize<T>(T message);

    T Deserialize<T>(byte[] data);
}
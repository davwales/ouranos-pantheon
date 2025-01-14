namespace Ouranos.Pantheon.Core.WebSockets.Serializers;

public interface IMessageSerializer
{
    byte[] Serialize<T>(T message);

    T Deserialize<T>(byte[] data);
}
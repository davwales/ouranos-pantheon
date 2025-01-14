namespace Ouranos.Pantheon.Core.WebSockets.Serializers;

public interface IMessageConverter
{
    byte[] Serialize(object data);

    object Deserialize(byte[] data, Type targetType);
}
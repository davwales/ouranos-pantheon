namespace Ouranos.Pantheon.Modules.Shared.WebSockets.Serializers;

public interface IMessageConverter
{
    byte[] Serialize(object data);

    object Deserialize(byte[] data, Type targetType);
}

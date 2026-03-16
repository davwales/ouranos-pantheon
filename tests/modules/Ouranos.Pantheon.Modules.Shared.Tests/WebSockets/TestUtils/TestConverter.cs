using Ouranos.Pantheon.Modules.Shared.WebSockets.Serializers;

namespace Ouranos.Pantheon.Modules.Shared.Tests.WebSockets.TestUtils;

public sealed class TestConverter : IMessageConverter
{
    public byte[] Serialize(object data)
    {
        return [];
    }

    public object Deserialize(byte[] data, Type targetType)
    {
        return data;
    }
}
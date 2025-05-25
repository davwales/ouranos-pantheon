using Ouranos.Pantheon.Core.WebSockets.Serializers;

namespace Ouranos.Pantheon.Core.WebSockets.Tests.TestUtils;

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
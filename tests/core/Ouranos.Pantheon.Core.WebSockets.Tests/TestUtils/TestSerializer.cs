using Ouranos.Pantheon.Core.WebSockets.Serializers;

namespace Ouranos.Pantheon.Core.WebSockets.Tests.TestUtils;

public sealed class TestSerializer : IMessageSerializer
{
    public byte[] Serialize<T>(T message)
    {
        return [];
    }

    public T Deserialize<T>(byte[] data)
    {
        return default!;
    }
}
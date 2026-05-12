using Ouranos.Pantheon.Modules.Shared.WebSockets.Serializers;

namespace Ouranos.Pantheon.Modules.Shared.Tests.WebSockets.TestUtils;

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

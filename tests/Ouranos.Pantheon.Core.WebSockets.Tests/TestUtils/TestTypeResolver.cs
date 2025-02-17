using Ouranos.Pantheon.Core.WebSockets.Serializers;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Core.WebSockets.Tests.TestUtils;

public sealed class TestTypeResolver : ITypeResolver
{
    public Type ResolveType(byte[] data)
    {
        return typeof(TestEntity);
    }
}
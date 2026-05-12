using Ouranos.Pantheon.Modules.Shared.WebSockets.Serializers;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Modules.Shared.Tests.WebSockets.TestUtils;

public sealed class TestTypeResolver : ITypeResolver
{
    public Type ResolveType(byte[] data)
    {
        return typeof(TestEntity);
    }
}

namespace Ouranos.Pantheon.Modules.Shared.WebSockets.Serializers.TypeResolvers;

public sealed class ConstantTypeResolver(Type type) : ITypeResolver
{
    public Type ResolveType(byte[] data)
    {
        return type;
    }
}
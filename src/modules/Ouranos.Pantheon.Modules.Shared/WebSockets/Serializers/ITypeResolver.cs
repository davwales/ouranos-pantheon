namespace Ouranos.Pantheon.Modules.Shared.WebSockets.Serializers;

public interface ITypeResolver
{
    Type ResolveType(byte[] data);
}
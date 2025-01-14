namespace Ouranos.Pantheon.Core.WebSockets.Serializers;

public interface ITypeResolver
{
    Type ResolveType(byte[] data);
}
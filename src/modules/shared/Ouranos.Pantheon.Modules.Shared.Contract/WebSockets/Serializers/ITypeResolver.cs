namespace Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.Serializers;

public interface ITypeResolver
{
    Type ResolveType(byte[] data);
}

using System.Text.Json;

namespace Ouranos.Pantheon.Core.WebSockets.Serializers.TypeResolvers;

public sealed class JsonTypeResolver : ITypeResolver
{
    private readonly string _discriminatorPath;
    private readonly Dictionary<string, Type> _typeMap;

    public JsonTypeResolver(
        string discriminatorPath,
        Dictionary<string, Type> typeMap
    )
    {
        _discriminatorPath = discriminatorPath;
        _typeMap = typeMap;
    }

    public Type ResolveType(byte[] data)
    {
        using var document = JsonDocument.Parse(data);
        var element = document.RootElement;

        if (element.ValueKind == JsonValueKind.Array)
        {
            return typeof(IList<object>);
        }

        if (_discriminatorPath.Split('.').Any(segment => !element.TryGetProperty(segment, out element)))
        {
            throw new InvalidOperationException($"Discriminator path '{_discriminatorPath}' not found.");
        }

        var discriminator = element.GetString();
        if (discriminator is null || !_typeMap.TryGetValue(discriminator, out var type))
        {
            throw new InvalidOperationException($"No type mapping found for discriminator: {discriminator}.");
        }

        return type;
    }
}
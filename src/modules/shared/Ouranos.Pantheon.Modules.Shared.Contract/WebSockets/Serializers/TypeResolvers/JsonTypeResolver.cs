using System.Text.Json;

namespace Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.Serializers.TypeResolvers;

public sealed record JsonTypeResolver(
    string DiscriminatorPath,
    IReadOnlyDictionary<string, Type> TypeMap
) : ITypeResolver
{
    public Type ResolveType(byte[] data)
    {
        using var document = JsonDocument.Parse(data);
        var element = document.RootElement;

        if (element.ValueKind == JsonValueKind.Array)
        {
            return typeof(IList<object>);
        }

        if (
            DiscriminatorPath
                .Split('.')
                .Any(segment => !element.TryGetProperty(segment, out element))
        )
        {
            throw new InvalidOperationException(
                $"Discriminator path '{DiscriminatorPath}' not found."
            );
        }

        var discriminator = element.GetString();
        if (discriminator is null || !TypeMap.TryGetValue(discriminator, out var type))
        {
            throw new InvalidOperationException(
                $"No type mapping found for discriminator: {discriminator}."
            );
        }

        return type;
    }
}

using System.Text.Json;
using System.Text.Json.Serialization;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Converters;

public sealed class IdJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType
            && typeToConvert.GetGenericTypeDefinition() == typeof(Id<>);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var entityType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(IdJsonConverter<>).MakeGenericType(entityType);
        var instance =
            Activator.CreateInstance(converterType)
            ?? throw new InvalidOperationException(
                $"Failed to create converter for type '{typeToConvert.Name}'."
            );
        return (JsonConverter)instance;
    }
}

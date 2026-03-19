using System.Text.Json;
using System.Text.Json.Serialization;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Converters;

public sealed class IdJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType &&
               typeToConvert.GetGenericTypeDefinition() == typeof(Id<>);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var entityType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(IdJsonConverter<>).MakeGenericType(entityType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

public sealed class IdJsonConverter<T> : JsonConverter<Id<T>>
{
    public override Id<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString() ?? throw new JsonException("Expected string for Id<T>");
        return Id<T>.Parse(value, null);
    }

    public override void Write(Utf8JsonWriter writer, Id<T> value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}

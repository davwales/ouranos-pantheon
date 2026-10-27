using System.Text.Json;
using System.Text.Json.Serialization;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Converters;

public sealed class IdJsonConverter<T> : JsonConverter<Id<T>>
{
    public override Id<T> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var value = reader.GetString() ?? throw new JsonException("Expected string for Id<T>");
        return Id<T>.Parse(value, null);
    }

    public override void Write(Utf8JsonWriter writer, Id<T> value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}

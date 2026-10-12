using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Converters;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Extensions;

public static class PropertyBuilderExtensions
{
    private static readonly JsonSerializerOptions JsonColumnOptions = new(
        JsonSerializerDefaults.Web
    )
    {
        AllowOutOfOrderMetadataProperties = true,
        Converters = { new IdJsonConverterFactory(), new JsonStringEnumConverter() },
    };

    public static PropertyBuilder<Id<T>> HasIdConversion<T>(this PropertyBuilder<Id<T>> builder)
    {
        return builder.HasConversion<IdConverter<T>>();
    }

    /// <summary>
    ///     Stores the value as <c>jsonb</c> in the same camelCase, string-enum shape the API uses,
    ///     so polymorphic records round-trip through their type discriminator.
    /// </summary>
    public static PropertyBuilder<T> HasJsonConversion<T>(this PropertyBuilder<T> builder)
        where T : class?
    {
        return builder
            .HasColumnType("jsonb")
            .HasConversion(
                value => SerializeJson(value),
                json => DeserializeJson<T>(json),
                new ValueComparer<T>(
                    (left, right) => SerializeJson(left) == SerializeJson(right),
                    value => SerializeJson(value).GetHashCode(StringComparison.Ordinal),
                    value => DeserializeJson<T>(SerializeJson(value))
                )
            );
    }

    private static string SerializeJson<T>(T value)
    {
        return JsonSerializer.Serialize(value, JsonColumnOptions);
    }

    private static T DeserializeJson<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, JsonColumnOptions)
            ?? throw new JsonException($"Column held null where {typeof(T).Name} was expected.");
    }
}

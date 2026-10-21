using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Converters;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;

internal static class StructuredOutputSchema
{
    private static readonly ConcurrentDictionary<Type, BinaryData> Cache = new();

    private static readonly JsonSerializerOptions ExportOptions = new(JsonSerializerDefaults.Web)
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        Converters = { new IdJsonConverterFactory(), new JsonStringEnumConverter() },
        NumberHandling = JsonNumberHandling.Strict,
    };

    private static readonly JsonSchemaExporterOptions ExporterOptions = new()
    {
        TreatNullObliviousAsNonNullable = true,
        TransformSchemaNode = (context, node) =>
        {
            var transformed = Transform(context, node);
            var description = DescriptionOf(context);
            if (description is not null && transformed is JsonObject described)
            {
                described["description"] = description;
            }

            return transformed;
        },
    };

    private static JsonNode Transform(JsonSchemaExporterContext context, JsonNode node)
    {
        // The exporter cannot see through a custom converter, so it would accept any value.
        if (IsId(context.TypeInfo.Type))
        {
            return new JsonObject { ["type"] = "string" };
        }

        if (IsId(Nullable.GetUnderlyingType(context.TypeInfo.Type)))
        {
            return new JsonObject { ["type"] = new JsonArray("string", "null") };
        }

        if (node is JsonObject obj && obj["properties"] is JsonObject properties)
        {
            var required = new JsonArray();
            foreach (var property in properties)
            {
                required.Add(property.Key);
            }

            obj["required"] = required;
            obj["additionalProperties"] = false;
        }

        return node;
    }

    // The exporter ignores [Description]. On a positional record the attribute lands on the
    // constructor parameter, not the property, so both are checked.
    private static string? DescriptionOf(JsonSchemaExporterContext context)
    {
        var property = context.PropertyInfo;
        var description =
            DescriptionOf(property?.AttributeProvider)
            ?? DescriptionOf(property?.AssociatedParameter?.AttributeProvider);
        if (description is not null)
        {
            return description;
        }

        var type = Nullable.GetUnderlyingType(context.TypeInfo.Type) ?? context.TypeInfo.Type;
        return type.IsEnum ? DescriptionOf(type) : null;
    }

    private static string? DescriptionOf(ICustomAttributeProvider? provider)
    {
        return provider
            ?.GetCustomAttributes(typeof(DescriptionAttribute), inherit: false)
            .OfType<DescriptionAttribute>()
            .FirstOrDefault()
            ?.Description;
    }

    private static bool IsId(Type? type)
    {
        return type is { IsGenericType: true } && type.GetGenericTypeDefinition() == typeof(Id<>);
    }

    public static BinaryData For<T>()
        where T : class
    {
        return Cache.GetOrAdd(
            typeof(T),
            static type =>
            {
                var schema = ExportOptions.GetJsonSchemaAsNode(type, ExporterOptions);
                return BinaryData.FromString(schema.ToJsonString());
            }
        );
    }
}

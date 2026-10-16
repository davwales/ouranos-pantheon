using System.Collections.Concurrent;
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
        },
    };

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

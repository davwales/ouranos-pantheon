using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;

internal static class StructuredOutputSchema
{
    private static readonly ConcurrentDictionary<Type, BinaryData> Cache = new();

    private static readonly JsonSerializerOptions ExportOptions = new(JsonSerializerDefaults.Web)
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
    };

    private static readonly JsonSchemaExporterOptions ExporterOptions = new()
    {
        TreatNullObliviousAsNonNullable = true,
        TransformSchemaNode = (_, node) =>
        {
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

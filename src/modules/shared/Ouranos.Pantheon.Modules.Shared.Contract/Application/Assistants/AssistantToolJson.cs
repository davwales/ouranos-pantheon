using System.Text.Json;
using System.Text.Json.Serialization;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Converters;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

internal static class AssistantToolJson
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new SignificantFiguresDecimalConverter(),
            new IdJsonConverterFactory(),
            new JsonStringEnumConverter(),
        },
    };

    public static string Serialize<TData>(AssistantData<TData> data)
        where TData : class
    {
        return data.Data is { } value
            ? JsonSerializer.Serialize(value, Options)
            : JsonSerializer.Serialize(new { note = data.Note }, Options);
    }
}

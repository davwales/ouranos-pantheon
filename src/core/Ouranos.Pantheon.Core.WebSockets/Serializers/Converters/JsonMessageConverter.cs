using System.Text;
using System.Text.Json;

namespace Ouranos.Pantheon.Core.WebSockets.Serializers.Converters;

public sealed class JsonMessageConverter(JsonSerializerOptions? options = null) : IMessageConverter
{
    private readonly JsonSerializerOptions _options = options ?? new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public byte[] Serialize(object data)
    {
        var json = JsonSerializer.Serialize(data, _options);
        return Encoding.UTF8.GetBytes(json);
    }

    public object Deserialize(byte[] data, Type targetType)
    {
        return JsonSerializer.Deserialize(data, targetType, _options)
               ?? throw new InvalidOperationException("Failed to convert json message.");
    }
}
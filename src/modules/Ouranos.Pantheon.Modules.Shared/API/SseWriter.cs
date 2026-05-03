using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Ouranos.Pantheon.Modules.Shared.API;

public static class SseWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Writes a Server-Sent Event to the HTTP response stream with the given data object
    /// serialized as JSON.
    /// </summary>
    public static async Task WriteEventAsync<T>(
        HttpResponse response,
        T data,
        CancellationToken cancellationToken = default
    )
    {
        var json = JsonSerializer.Serialize(data, SerializerOptions);
        await response.WriteAsync($"data: {json}\n\n", cancellationToken);
        await response.Body.FlushAsync(cancellationToken);
    }

    /// <summary>
    /// Configures the HTTP response headers for Server-Sent Events.
    /// </summary>
    public static void SetSseHeaders(HttpResponse response)
    {
        response.Headers.ContentType = "text/event-stream";
        response.Headers.CacheControl = "no-cache";
        response.Headers.Connection = "keep-alive";
    }
}
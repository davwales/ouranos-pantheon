using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Requests;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Images;

/// <summary>
/// Converts between Pantheon's image types and OuranosMl's streamed
/// <c>POST /images/generations</c> and <c>POST /images/edits</c> wire formats.
/// </summary>
internal static class ImageGenerationMapping
{
    private static readonly JsonSerializerOptions SnakeCaseLenientOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public static HttpContent ToContent(GenerateImageRequest request)
    {
        var body = new GenerateImagesBody(
            request.Prompt,
            request.Model,
            N: 1,
            Size: $"{request.Width}x{request.Height}",
            Stream: true,
            Save: false,
            Previews: false
        );

        return ToJsonContent(body);
    }

    public static HttpContent ToContent(EditImageRequest request)
    {
        var body = new EditImagesBody(
            request.Prompt,
            request.Model,
            N: 1,
            Image: Convert.ToBase64String(request.Image),
            Strength: request.Strength,
            Stream: true,
            Save: false,
            Previews: false
        );

        return ToJsonContent(body);
    }

    /// <summary>
    /// Maps one SSE event; returns null for events Pantheon ignores, such as status and progress,
    /// and throws for an error event.
    /// </summary>
    public static ImageGenerationEventDto? ToEvent(string eventType, string data)
    {
        return eventType switch
        {
            ImageGenerationEventTypes.Completed => ToCompletedImage(
                Deserialize<ImageGenerationCompletedEvent>(data)
            ),
            ImageGenerationEventTypes.Done => new ImageGenerationDoneDto(
                Deserialize<ImageGenerationDoneEvent>(data).Model
            ),
            ImageGenerationEventTypes.Error => throw ToException(
                Deserialize<ImageGenerationErrorEvent>(data)
            ),
            _ => null,
        };
    }

    public static async Task<OuranosMlRequestException> ToExceptionAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken
    )
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        OuranosMlError? error = null;
        try
        {
            error = JsonSerializer
                .Deserialize<OuranosMlErrorResponse>(content, SnakeCaseLenientOptions)
                ?.Error;
        }
        catch (JsonException)
        {
            // A non-JSON body (e.g. from a proxy) still gets the status-based message below.
        }

        return new OuranosMlRequestException(
            error?.Type ?? "http_error",
            error?.Message ?? $"OuranosMl returned {(int)response.StatusCode}.",
            response.StatusCode
        );
    }

    private static StringContent ToJsonContent<T>(T body)
    {
        return new StringContent(
            JsonSerializer.Serialize(body, SnakeCaseLenientOptions),
            Encoding.UTF8,
            MediaTypeNames.Application.Json
        );
    }

    private static T Deserialize<T>(string data)
        where T : class
    {
        return JsonSerializer.Deserialize<T>(data, SnakeCaseLenientOptions)
            ?? throw new JsonException($"OuranosMl sent an empty {typeof(T).Name}.");
    }

    private static OuranosMlRequestException ToException(ImageGenerationErrorEvent error)
    {
        return new OuranosMlRequestException(error.Code, error.Message);
    }

    private static ImageGenerationCompletedDto ToCompletedImage(ImageGenerationCompletedEvent image)
    {
        var size = image.Size.Split('x');
        if (
            size.Length != 2
            || !int.TryParse(size[0], out var width)
            || !int.TryParse(size[1], out var height)
        )
        {
            throw new OuranosMlRequestException(
                "invalid_image",
                "The generated image had an unreadable size."
            );
        }

        return new ImageGenerationCompletedDto(
            Convert.FromBase64String(image.B64Json),
            ToMediaType(image.OutputFormat),
            width,
            height,
            image.Seed
        );
    }

    private static string ToMediaType(string format)
    {
        return format.ToLowerInvariant() switch
        {
            "jpg" or "jpeg" => MediaTypeNames.Image.Jpeg,
            "webp" => MediaTypeNames.Image.Webp,
            "gif" => MediaTypeNames.Image.Gif,
            "png" => MediaTypeNames.Image.Png,
            _ => throw new OuranosMlRequestException(
                "invalid_image",
                $"The generated image had an unsupported format '{format}'."
            ),
        };
    }
}

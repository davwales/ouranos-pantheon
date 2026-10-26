using System.Net;
using System.Text.Json.Nodes;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Images;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Requests;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Tests.Infra.OuranosMachineLearning.Images;

public sealed class ImageGenerationMappingTests
{
    private static string Completed(string size, string format)
    {
        var content = Convert.ToBase64String([1, 2, 3]);
        return $$"""{"type":"image_generation.completed","b64_json":"{{content}}","seed":7,"size":"{{size}}","output_format":"{{format}}"}""";
    }

    [Fact]
    public async Task ToContent_ShouldRequestOneStreamedUnsavedImageOfTheGivenSize()
    {
        // Act
        using var content = ImageGenerationMapping.ToContent(
            new GenerateImageRequest("A fox", 832, 1216, "flux")
        );

        // Assert
        var body = JsonNode.Parse(await content.ReadAsStringAsync()).ShouldNotBeNull();
        body["prompt"]!.GetValue<string>().ShouldBe("A fox");
        body["model"]!.GetValue<string>().ShouldBe("flux");
        body["n"]!.GetValue<int>().ShouldBe(1);
        body["size"]!.GetValue<string>().ShouldBe("832x1216");
        body["stream"]!.GetValue<bool>().ShouldBeTrue();
        body["save"]!.GetValue<bool>().ShouldBeFalse();
        body["previews"]!.GetValue<bool>().ShouldBeFalse();
    }

    [Fact]
    public async Task ToContent_WhenEditing_ShouldSendTheImageAndStrengthWithoutASize()
    {
        // Act
        using var content = ImageGenerationMapping.ToContent(
            new EditImageRequest("A fox at night", [1, 2, 3], 0.3f, "flux")
        );

        // Assert
        var body = JsonNode.Parse(await content.ReadAsStringAsync()).ShouldNotBeNull();
        body["prompt"]!.GetValue<string>().ShouldBe("A fox at night");
        body["model"]!.GetValue<string>().ShouldBe("flux");
        body["image"]!.GetValue<string>().ShouldBe(Convert.ToBase64String([1, 2, 3]));
        body["strength"]!.GetValue<float>().ShouldBe(0.3f);
        body["n"]!.GetValue<int>().ShouldBe(1);
        body["stream"]!.GetValue<bool>().ShouldBeTrue();
        body["save"]!.GetValue<bool>().ShouldBeFalse();
        body["previews"]!.GetValue<bool>().ShouldBeFalse();
        body.AsObject().ContainsKey("size").ShouldBeFalse();
    }

    [Theory]
    [InlineData("png", "image/png")]
    [InlineData("JPG", "image/jpeg")]
    [InlineData("webp", "image/webp")]
    public void ToEvent_WhenImageIsCompleted_ShouldDecodeItAndItsMediaType(
        string format,
        string mediaType
    )
    {
        // Act
        var imageEvent = ImageGenerationMapping.ToEvent(
            ImageGenerationEventTypes.Completed,
            Completed("64x32", format)
        );

        // Assert
        var completed = imageEvent.ShouldBeOfType<ImageGenerationCompletedDto>();
        completed.Content.ShouldBe(new byte[] { 1, 2, 3 });
        completed.MediaType.ShouldBe(mediaType);
        completed.Width.ShouldBe(64);
        completed.Height.ShouldBe(32);
        completed.Seed.ShouldBe(7);
    }

    [Theory]
    [InlineData("64", "png")]
    [InlineData("axb", "png")]
    [InlineData("64x32", "tiff")]
    public void ToEvent_WhenImageIsUnreadable_ShouldThrow(string size, string format)
    {
        // Act & Assert
        Should
            .Throw<OuranosMlRequestException>(() =>
                ImageGenerationMapping.ToEvent(
                    ImageGenerationEventTypes.Completed,
                    Completed(size, format)
                )
            )
            .Type.ShouldBe("invalid_image");
    }

    [Fact]
    public void ToEvent_WhenEventIsNotNeeded_ShouldReturnNull()
    {
        // Act
        var imageEvent = ImageGenerationMapping.ToEvent(
            "image_generation.progress",
            """{"type":"image_generation.progress","total_percent":0.5}"""
        );

        // Assert
        imageEvent.ShouldBeNull();
    }

    [Fact]
    public async Task ToExceptionAsync_WhenBodyIsNotJson_ShouldUseTheStatus()
    {
        // Arrange
        using var response = new HttpResponseMessage(HttpStatusCode.BadGateway)
        {
            Content = new StringContent("<html>Bad gateway</html>"),
        };

        // Act
        var exception = await ImageGenerationMapping.ToExceptionAsync(
            response,
            CancellationToken.None
        );

        // Assert
        exception.Type.ShouldBe("http_error");
        exception.Message.ShouldBe("OuranosMl returned 502.");
        exception.StatusCode.ShouldBe(HttpStatusCode.BadGateway);
    }
}

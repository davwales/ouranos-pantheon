using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Conversations.GenerateCompletion.Schemas;

public sealed class ContentChunkResponseTests
{
    [Fact]
    public void ContentChunkResponse_Content_ShouldBeSet()
    {
        // Arrange
        var expectedContent = "Hello, world!";

        // Act
        var response = new ContentChunkResponse(expectedContent);

        // Assert
        response.Content.ShouldBe(expectedContent);
    }

    [Fact]
    public void ContentChunkResponse_ShouldBeGenerateCompletionResponse()
    {
        // Arrange & Act
        var response = new ContentChunkResponse("test");

        // Assert
        response.ShouldBeAssignableTo<GenerateCompletionResponse>();
    }
}

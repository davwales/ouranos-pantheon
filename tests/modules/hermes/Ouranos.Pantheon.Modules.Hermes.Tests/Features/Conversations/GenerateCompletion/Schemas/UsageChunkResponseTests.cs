using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Conversations.GenerateCompletion.Schemas;

public sealed class UsageChunkResponseTests
{
    [Fact]
    public void UsageChunkResponse_Properties_ShouldBeSet()
    {
        // Arrange
        const int inputTokens = 100;
        const int outputTokens = 50;
        const int totalTokens = 150;

        // Act
        var response = new UsageChunkResponse(inputTokens, outputTokens, totalTokens);

        // Assert
        response.InputTokens.ShouldBe(inputTokens);
        response.OutputTokens.ShouldBe(outputTokens);
        response.TotalTokens.ShouldBe(totalTokens);
    }

    [Fact]
    public void UsageChunkResponse_ShouldBeGenerateCompletionResponse()
    {
        // Arrange & Act
        var response = new UsageChunkResponse(1, 2, 3);

        // Assert
        response.ShouldBeAssignableTo<GenerateCompletionResponse>();
    }
}

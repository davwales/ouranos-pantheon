using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Dtos;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Requests;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Infra.OuranosMachineLearning.Requests;

public sealed class GenerateCompletionRequestTests
{
    [Fact]
    public void Constructor_SetsExpectedValues()
    {
        // Arrange
        var fixture = new Fixture();
        var expectedMessages = fixture.CreateMany<MessageDto>().ToList();

        // Act
        var request = new GenerateCompletionRequest(expectedMessages);

        // Assert
        request.Messages.ShouldBe(expectedMessages);
    }
}
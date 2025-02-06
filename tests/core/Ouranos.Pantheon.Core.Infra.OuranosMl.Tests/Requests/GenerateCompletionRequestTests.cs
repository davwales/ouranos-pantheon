using Ouranos.Pantheon.Core.Infra.OuranosMl.Dtos;
using Ouranos.Pantheon.Core.Infra.OuranosMl.Requests;

namespace Ouranos.Pantheon.Core.Infra.OuranosMl.Tests.Requests;

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
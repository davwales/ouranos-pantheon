using Ouranos.Pantheon.Core.Infra.OuranosMl.Dtos;

namespace Ouranos.Pantheon.Core.Infra.OuranosMl.Tests.Dtos;

public sealed class MessageDtoTests
{
    [Fact]
    public void Constructor_SetsExpectedValues()
    {
        // Arrange
        var fixture = new Fixture();
        var expectedContent = fixture.Create<string>();
        var expectedRole = fixture.Create<RoleDto>();

        // Act
        var message = new MessageDto(expectedContent, expectedRole);

        // Assert
        message.Content.ShouldBe(expectedContent);
        message.Role.ShouldBe(expectedRole);
    }
}
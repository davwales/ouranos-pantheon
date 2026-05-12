using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Application.Common;

public sealed class IdResponseTests
{
    [Fact]
    public void Constructor_ShouldSetExpectedValues()
    {
        // Arrange
        var fixture = new Fixture();
        var expectedId = fixture.Create<Id<TestEntity>>();

        // Act
        var response = new IdResponse<TestEntity>(expectedId);

        // Assert
        response.Id.ShouldBe(expectedId);
    }
}

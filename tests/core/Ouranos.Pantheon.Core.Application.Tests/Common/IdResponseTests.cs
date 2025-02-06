using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Core.Application.Tests.Common;

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
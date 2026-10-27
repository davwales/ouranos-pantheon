using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Shared.Domain.Conversations;

public sealed class RoleExtensionsTests
{
    [Theory]
    [InlineData(Role.System, RoleDto.System)]
    [InlineData(Role.User, RoleDto.User)]
    [InlineData(Role.Assistant, RoleDto.Assistant)]
    [InlineData(Role.Summary, RoleDto.Assistant)]
    public void ToRoleDto_ShouldMapEachRole(Role role, RoleDto expected)
    {
        // Act
        var result = role.ToRoleDto();

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void ToRoleDto_WhenRoleIsUnknown_ShouldThrowArgumentOutOfRangeException()
    {
        // Act & Assert
        Should.Throw<ArgumentOutOfRangeException>(() => ((Role)99).ToRoleDto());
    }
}

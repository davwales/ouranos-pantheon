using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Osrs.Models;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.DataLoaders.Osrs;

public sealed class MappingTests
{
    private static Mapping MakeMapping(object members)
    {
        return new(1234, "Test Item", "icon.png", "A test item", members, 100, 200, 1000, 500);
    }

    [Fact]
    public void IsMembers_WhenMembersIsBoolTrue_ReturnsTrue()
    {
        // Act
        var result = MakeMapping(true).IsMembers;

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsMembers_WhenMembersIsBoolFalse_ReturnsFalse()
    {
        // Act
        var result = MakeMapping(false).IsMembers;

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsMembers_WhenMembersIsStringTrue_ReturnsTrue()
    {
        // Act
        var result = MakeMapping("true").IsMembers;

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsMembers_WhenMembersIsStringFalse_ReturnsFalse()
    {
        // Act
        var result = MakeMapping("false").IsMembers;

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsMembers_WhenMembersIsUnparsableString_ReturnsTrue()
    {
        // Act
        var result = MakeMapping("yes").IsMembers;

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsMembers_WhenMembersIsNeitherBoolNorString_ReturnsTrue()
    {
        // Act
        var result = MakeMapping(42).IsMembers;

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void Mapping_AllProperties_ShouldBeAccessible()
    {
        // Arrange & Act
        var mapping = MakeMapping(true);

        // Assert
        mapping.Id.ShouldBe(1234);
        mapping.Name.ShouldBe("Test Item");
        mapping.Icon.ShouldBe("icon.png");
        mapping.Examine.ShouldBe("A test item");
        mapping.LowAlch.ShouldBe(100);
        mapping.HighAlch.ShouldBe(200);
        mapping.Limit.ShouldBe(1000);
        mapping.Value.ShouldBe(500);
        mapping.Members.ShouldNotBeNull();
    }
}

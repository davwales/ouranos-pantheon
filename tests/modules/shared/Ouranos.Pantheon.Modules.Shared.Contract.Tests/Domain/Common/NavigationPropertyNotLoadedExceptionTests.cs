using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Domain.Common;

public sealed class NavigationPropertyNotLoadedExceptionTests
{
    private sealed class SomeEntity;

    [Fact]
    public void Constructor_WhenCreated_ShouldContainEntityTypeName()
    {
        // Act
        var exception = new NavigationPropertyNotLoadedException<SomeEntity>();

        // Assert
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain(nameof(SomeEntity));
    }

    [Fact]
    public void Constructor_WhenCreated_ShouldBeInvalidOperationException()
    {
        // Act
        var exception = new NavigationPropertyNotLoadedException<SomeEntity>();

        // Assert
        exception.ShouldBeOfType<NavigationPropertyNotLoadedException<SomeEntity>>();
        exception.ShouldBeAssignableTo<InvalidOperationException>();
    }
}

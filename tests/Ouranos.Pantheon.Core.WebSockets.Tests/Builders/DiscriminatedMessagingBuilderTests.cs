using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.WebSockets.Builders;
using Ouranos.Pantheon.Core.WebSockets.Listeners;
using Ouranos.Pantheon.Core.WebSockets.Tests.TestUtils;
using Ouranos.Pantheon.Tests.Utils;
using Ouranos.Pantheon.Tests.Utils.Shouldly;

namespace Ouranos.Pantheon.Core.WebSockets.Tests.Builders;

public sealed class DiscriminatedMessagingBuilderTests
{
    private readonly ServiceCollection _services = [];

    [Fact]
    public void Constructor_ShouldSetExpectedValues()
    {
        // Arrange
        const string expectedDiscriminator = "myDiscriminator";

        // Act
        var builder = new DiscriminatedMessagingBuilder<TestEntity>(expectedDiscriminator, _services);

        // Assert
        builder.Discriminator.ShouldBe(expectedDiscriminator);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WhenInvalidDiscriminator_ShouldThrowArgumentException(string? discriminator)
    {
        // Act
        var create = () =>
            new DiscriminatedMessagingBuilder<TestEntity>(discriminator!, _services);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhenNullServices_ShouldThrowArgumentException()
    {
        // Act
        var create = () =>
            new DiscriminatedMessagingBuilder<TestEntity>("myDiscriminator", null!);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void UseListener_ShouldRegisterExpectedService()
    {
        // Arrange
        var builder = GivenBuilder();

        // Act
        builder.UseListener<TestListener>();

        // Assert
        _services.ShouldContainService<TestListener>(ServiceLifetime.Transient);
    }

    [Fact]
    public void UseListener_ShouldReturnBuilder()
    {
        // Arrange
        var builder = GivenBuilder();

        // Act
        var actualResult = builder.UseListener<TestListener>();

        // Assert
        actualResult.ShouldBe(builder);
    }

    [Fact]
    public void RegisterListeners_ShouldRegisterListenersWithRegistry()
    {
        // Arrange
        var builder = GivenBuilder();
        builder.UseListener<TestListener>();
        var provider = _services.BuildServiceProvider();
        var registry = Substitute.For<IListenerRegistry>();

        // Act
        builder.RegisterListeners(registry, provider);

        // Assert
        registry.Received(1).RegisterListener(
            Arg.Is<IListener<TestEntity>>(x => x is TestListener)
        );
    }

    private DiscriminatedMessagingBuilder<TestEntity> GivenBuilder()
    {
        return new DiscriminatedMessagingBuilder<TestEntity>("myDiscriminator", _services);
    }
}
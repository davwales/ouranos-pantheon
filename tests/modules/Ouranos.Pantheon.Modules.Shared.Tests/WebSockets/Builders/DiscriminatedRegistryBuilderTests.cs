using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Shared.Tests.WebSockets.TestUtils;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Builders;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Listeners;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Serializers;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Serializers.TypeResolvers;
using Ouranos.Pantheon.Tests.Utils;
using Ouranos.Pantheon.Tests.Utils.Shouldly;

namespace Ouranos.Pantheon.Modules.Shared.Tests.WebSockets.Builders;

public sealed class DiscriminatedRegistryBuilderTests
{
    private readonly ServiceCollection _services = [];

    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Act
        var builder = new DiscriminatedRegistryBuilder(_services);

        // Assert
        builder.DiscriminatorPath.ShouldBeNull();
        builder.MessagingBuilders.ShouldBeEmpty();
    }

    [Fact]
    public void Constructor_WhenNullServices_ThrowsArgumentException()
    {
        // Act
        var ctor = () => new DiscriminatedRegistryBuilder(null!);

        // Assert
        ctor.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void UseDiscriminatorPath_WhenValid_ShouldSetDiscriminatorPath()
    {
        // Arrange
        const string expectedDiscriminatorPath = "Prop1:Prop2:_t";
        var builder = GivenBuilder();

        // Act
        builder.UseDiscriminatorPath(expectedDiscriminatorPath);

        // Assert
        builder.DiscriminatorPath.ShouldBe(expectedDiscriminatorPath);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void UseDiscriminatorPath_WhenInvalid_ShouldThrowArgumentException(string? discriminatorPath)
    {
        // Arrange
        var builder = GivenBuilder();

        // Act
        var use = () => builder.UseDiscriminatorPath(discriminatorPath!);

        // Assert
        use.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void UseMessage_ShouldConfigureMessage()
    {
        // Arrange
        var builder = GivenBuilder();

        // Act
        builder.UseMessage<TestEntity>("test", _ => { });

        // Assert
        builder.MessagingBuilders.Count.ShouldBe(1);
        builder.MessagingBuilders.Keys.ShouldContain(typeof(TestEntity));
    }

    [Fact]
    public void UseMessage_ShouldReturnBuilder()
    {
        // Arrange
        var builder = GivenBuilder();

        // Act
        var actualResult = builder.UseMessage<TestEntity>(
            "test",
            _ => { }
        );

        // Assert
        actualResult.ShouldBe(builder);
    }

    [Fact]
    public void Build_ShouldRegisterExpectedServices()
    {
        // Arrange
        var builder = GivenBuilder();
        builder.UseDiscriminatorPath("myDiscriminatorPath");

        // Act
        builder.Build();

        // Assert
        _services.ShouldContainService<ITypeResolver>(ServiceLifetime.Transient);
        _services.ShouldContainService<IListenerRegistry>(ServiceLifetime.Singleton);
    }

    [Fact]
    public void Build_ShouldCreateExpectedTypeResolver()
    {
        // Arrange
        const string discriminatorPath = "myDiscriminatorPath";
        const string messageDiscriminator = "test";
        var builder = GivenBuilder();

        builder.UseDiscriminatorPath(discriminatorPath);
        builder.UseMessage<TestEntity>(messageDiscriminator, _ => { });

        // Act
        builder.Build();

        // Assert
        var provider = _services.BuildServiceProvider();
        var typeResolver = provider.GetService<ITypeResolver>();

        typeResolver.ShouldNotBeNull();

        var jsonResolver = typeResolver.ShouldBeOfType<JsonTypeResolver>();
        jsonResolver.DiscriminatorPath.ShouldBe(discriminatorPath);
        jsonResolver.TypeMap.Count.ShouldBe(1);
        jsonResolver.TypeMap.Keys.First().ShouldBe(messageDiscriminator);
    }

    [Fact]
    public void Build_ShouldCreateExpectedListenerRegistry()
    {
        // Arrange
        var builder = GivenBuilder();
        var serializer = Substitute.For<IMessageSerializer>();

        builder.UseMessage<TestEntity>(
            "type",
            x => x.UseListener<TestListener>()
        );

        builder.UseDiscriminatorPath("myDiscriminatorPath");
        _services.AddScoped<IMessageSerializer>(_ => serializer);

        // Act
        builder.Build();

        // Assert
        var provider = _services.BuildServiceProvider();

        var listenerRegistry = provider.GetService<IListenerRegistry>();
        listenerRegistry.ShouldNotBeNull();

        var registry = listenerRegistry.ShouldBeOfType<ListenerRegistry>();
        registry.ShouldNotBeNull();
        registry.Listeners.Count.ShouldBe(1);
        registry.Listeners.Keys.ShouldContain(typeof(TestEntity));
        registry.Listeners[typeof(TestEntity)].ShouldNotBeNull();
        registry.Listeners[typeof(TestEntity)].Count.ShouldBe(1);
    }

    [Fact]
    public void Build_WhenDiscriminatorPathNotSet_ShouldThrowArgumentException()
    {
        // Arrange
        var builder = GivenBuilder();

        // Act
        var build = () => builder.Build();

        // Assert
        build.ShouldThrow<ArgumentException>();
    }

    private DiscriminatedRegistryBuilder GivenBuilder()
    {
        return new DiscriminatedRegistryBuilder(_services);
    }
}
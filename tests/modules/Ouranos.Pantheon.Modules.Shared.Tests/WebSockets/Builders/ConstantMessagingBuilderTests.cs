using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Shared.Tests.WebSockets.TestUtils;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Builders;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Listeners;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Serializers;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Serializers.TypeResolvers;
using Ouranos.Pantheon.Tests.Utils;
using Ouranos.Pantheon.Tests.Utils.Shouldly;

namespace Ouranos.Pantheon.Modules.Shared.Tests.WebSockets.Builders;

public sealed class ConstantMessagingBuilderTests
{
    private readonly ConstantMessagingBuilder<TestEntity> _builder;
    private readonly IServiceCollection _services;

    public ConstantMessagingBuilderTests()
    {
        _services = new ServiceCollection();
        _builder = new ConstantMessagingBuilder<TestEntity>(_services);
    }

    [Fact]
    public void UseListener_ShouldReturnBuilder()
    {
        // Act
        var actualResult = _builder.UseListener<TestListener>();

        // Assert
        actualResult.ShouldBe(_builder);
    }

    [Fact]
    public void UseListener_ShouldRegisterListener()
    {
        // Act
        _builder.UseListener<TestListener>();

        // Assert
        _services.ShouldContainService<TestListener>(ServiceLifetime.Transient);
    }

    [Fact]
    public void Build_ShouldRegisterExpectedServices()
    {
        // Arrange
        _builder.UseListener<TestListener>();

        // Act
        _builder.Build();

        // Assert
        _services.ShouldContainService<ITypeResolver>(ServiceLifetime.Transient);
        _services.ShouldContainService<IListenerRegistry>(ServiceLifetime.Singleton);
    }

    [Fact]
    public void Build_ShouldUseConstantTypeResolver()
    {
        // Act
        _builder.Build();

        // Assert
        var provider = _services.BuildServiceProvider();
        var typeResolver = provider.GetService<ITypeResolver>();
        typeResolver.ShouldNotBeNull();
        typeResolver.ShouldBeOfType<ConstantTypeResolver>();
    }

    [Fact]
    public void Build_ShouldCreateExpectedListenerRegistry()
    {
        // Arrange
        _services.AddScoped<IMessageSerializer>(_ => Substitute.For<IMessageSerializer>());
        _builder.UseListener<TestListener>();

        // Act
        _builder.Build();

        // Assert
        var provider = _services.BuildServiceProvider();
        var listenerRegistry = provider.GetService<IListenerRegistry>();
        listenerRegistry.ShouldNotBeNull();
        listenerRegistry.Listeners.Count.ShouldBe(1);
        listenerRegistry.Listeners.Keys.ShouldContain(typeof(TestEntity));
        listenerRegistry.Listeners[typeof(TestEntity)].Count.ShouldBe(1);
    }
}

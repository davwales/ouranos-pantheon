using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Shared.Tests.WebSockets.TestUtils;
using Ouranos.Pantheon.Modules.Shared.WebSockets;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Builders;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Serializers;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Serializers.Converters;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Serializers.TypeResolvers;
using Ouranos.Pantheon.Modules.Shared.WebSockets.WebSocketClients;
using Ouranos.Pantheon.Tests.Utils;
using Ouranos.Pantheon.Tests.Utils.Shouldly;

namespace Ouranos.Pantheon.Modules.Shared.Tests.WebSockets.Builders;

public sealed class WebSocketClientBuilderTests
{
    private readonly ServiceCollection _services = [];

    [Fact]
    public void Constructor_WhenPopulatedOptions_ShouldSetExpectedValues()
    {
        // Arrange
        var fixture = new Fixture();
        var options = fixture.Create<WebSocketOptions>();

        // Act
        var builder = new WebSocketClientBuilder(_services, options);

        // Assert
        builder.BufferSize.ShouldBe(options.BufferSize);
        builder.Host.ShouldBe(options.Host);
    }

    [Fact]
    public void Constructor_WhenNullOptions_ShouldSetDefaultValues()
    {
        // Act
        var builder = new WebSocketClientBuilder(_services);

        // Assert
        builder.BufferSize.ShouldBe((uint)4096);
        builder.Host.ShouldBe(string.Empty);
    }

    [Fact]
    public void UseBufferSize_ShouldReturnBuilder()
    {
        // Arrange
        var builder = new WebSocketClientBuilder(_services);

        // Act
        var actualResult = builder.UseBufferSize(1234);

        // Assert
        actualResult.ShouldBe(builder);
    }

    [Fact]
    public void UseBufferSize_WhenValid_ShouldSetBufferSizeValue()
    {
        // Arrange
        const uint expectedBufferSize = 1234;
        var builder = new WebSocketClientBuilder(_services);

        // Act
        builder.UseBufferSize(expectedBufferSize);

        // Assert
        builder.BufferSize.ShouldBe(expectedBufferSize);
    }

    [Fact]
    public void ConfigureHost_ShouldReturnBuilder()
    {
        // Arrange
        var builder = new WebSocketClientBuilder(_services);

        // Act
        var actualResult = builder.ConfigureHost("localhost:1234");

        // Assert
        actualResult.ShouldBe(builder);
    }

    [Fact]
    public void ConfigureHost_WhenValid_ShouldSetBufferSizeValue()
    {
        // Arrange
        const string expectedHost = "localhost:1234";
        var builder = new WebSocketClientBuilder(_services);

        // Act
        builder.ConfigureHost(expectedHost);

        // Assert
        builder.Host.ShouldBe(expectedHost);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ConfigureHost_WhenInvalid_ShouldThrowArgumentException(string? host)
    {
        // Arrange
        var builder = new WebSocketClientBuilder(_services);

        // Act
        var use = () => builder.ConfigureHost(host!);

        // Assert
        use.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void UseConstantMessage_ShouldSetExpectedConfiguration()
    {
        // Arrange
        var builder = new WebSocketClientBuilder(_services);

        // Act
        builder.UseConstantMessage<TestEntity>(_ => { });

        // Assert
        builder.ConfigureMessaging.ShouldNotBeNull();
    }

    [Fact]
    public void UseConstantMessage_ShouldReturnBuilder()
    {
        // Arrange
        var builder = new WebSocketClientBuilder(_services);

        // Act
        var actualResult = builder.UseConstantMessage<TestEntity>(_ => { });

        // Assert
        actualResult.ShouldBe(builder);
    }

    [Fact]
    public void UseDiscriminatedMessages_ShouldSetExpectedConfiguration()
    {
        // Arrange
        var builder = new WebSocketClientBuilder(_services);

        // Act
        builder.UseDiscriminatedMessages(_ => { });

        // Assert
        builder.ConfigureMessaging.ShouldNotBeNull();
    }

    [Fact]
    public void UseDiscriminatedMessages_ShouldReturnBuilder()
    {
        // Arrange
        var builder = new WebSocketClientBuilder(_services);

        // Act
        var actualResult = builder.UseDiscriminatedMessages(_ => { });

        // Assert
        actualResult.ShouldBe(builder);
    }

    [Fact]
    public void UseSerializer_ShouldRegisterExpectedService()
    {
        // Arrange
        var builder = new WebSocketClientBuilder(_services);

        // Act
        builder.UseSerializer<TestSerializer>();

        // Assert
        _services.ShouldContainServiceWithImplementation<IMessageSerializer, TestSerializer>(
            ServiceLifetime.Singleton
        );
    }

    [Fact]
    public void UseSerializer_ShouldReturnBuilder()
    {
        // Arrange
        var builder = new WebSocketClientBuilder(_services);

        // Act
        var actualResult = builder.UseSerializer<TestSerializer>();

        // Assert
        actualResult.ShouldBe(builder);
    }

    [Fact]
    public void UseInitializer_ShouldRegisterExpectedService()
    {
        // Arrange
        var builder = new WebSocketClientBuilder(_services);

        // Act
        builder.UseInitializer<TestInitializer>();

        // Assert
        _services.ShouldContainServiceWithImplementation<IWebSocketInitializer, TestInitializer>(
            ServiceLifetime.Transient
        );
    }

    [Fact]
    public void UseInitializer_ShouldReturnBuilder()
    {
        // Arrange
        var builder = new WebSocketClientBuilder(_services);

        // Act
        var actualResult = builder.UseInitializer<TestInitializer>();

        // Assert
        actualResult.ShouldBe(builder);
    }

    [Fact]
    public void UseConverter_ShouldRegisterExpectedService()
    {
        // Arrange
        var builder = new WebSocketClientBuilder(_services);

        // Act
        builder.UseConverter<TestConverter>();

        // Assert
        _services.ShouldContainServiceWithImplementation<IMessageConverter, TestConverter>(
            ServiceLifetime.Singleton
        );
    }

    [Fact]
    public void UseConverter_ShouldReturnBuilder()
    {
        // Arrange
        var builder = new WebSocketClientBuilder(_services);

        // Act
        var actualResult = builder.UseConverter<TestConverter>();

        // Assert
        actualResult.ShouldBe(builder);
    }

    [Fact]
    public void UseTypeResolver_ShouldRegisterExpectedService()
    {
        // Arrange
        var builder = new WebSocketClientBuilder(_services);

        // Act
        builder.UseTypeResolver<TestTypeResolver>();

        // Assert
        _services.ShouldContainServiceWithImplementation<ITypeResolver, TestTypeResolver>(
            ServiceLifetime.Singleton
        );
    }

    [Fact]
    public void UseTypeResolver_ShouldReturnBuilder()
    {
        // Arrange
        var builder = new WebSocketClientBuilder(_services);

        // Act
        var actualResult = builder.UseTypeResolver<TestTypeResolver>();

        // Assert
        actualResult.ShouldBe(builder);
    }

    [Fact]
    public void Build_ShouldRegisterExpectedServices()
    {
        // Arrange
        var builder = new WebSocketClientBuilder(_services);

        // Act
        builder.Build();

        // Assert
        _services.ShouldContainServiceWithImplementation<ITypeResolver, JsonTypeResolver>(
            ServiceLifetime.Singleton
        );

        _services.ShouldContainServiceWithImplementation<IMessageConverter, JsonMessageConverter>(
            ServiceLifetime.Singleton
        );

        _services.ShouldContainServiceWithImplementation<IMessageSerializer, MessageSerializer>(
            ServiceLifetime.Singleton
        );

        _services.ShouldContainService<IWebSocketClient>(ServiceLifetime.Singleton);
    }

    [Fact]
    public void Build_WhenConstantMessageConfigured_ShouldInvokeConfigureMessaging()
    {
        // Arrange
        var builder = new WebSocketClientBuilder(_services);
        builder.UseConstantMessage<TestEntity>(_ => { });

        // Act
        var act = () => builder.Build();

        // Assert
        act.ShouldNotThrow();
    }

    [Fact]
    public void Build_WhenDiscriminatedMessagesConfigured_ShouldInvokeConfigureMessaging()
    {
        // Arrange
        var builder = new WebSocketClientBuilder(_services);
        builder.UseDiscriminatedMessages(b => b.UseDiscriminatorPath("type"));

        // Act
        var act = () => builder.Build();

        // Assert
        act.ShouldNotThrow();
    }
}

using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Shared.API.ExceptionHandlers;
using Ouranos.Pantheon.Modules.Shared.API.Extensions;

namespace Ouranos.Pantheon.Modules.Shared.Tests.API.Extensions;

public sealed class RestExtensionsTests
{
    [Fact]
    public void ConfigureRest_ShouldAddProblemDetails()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        services.ConfigureRest(configuration);

        // Assert
        services.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void ConfigureRest_WhenJsonOptionsResolved_ShouldApplySerializerConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        services.ConfigureRest(configuration);

        // Act
        var provider = services.BuildServiceProvider();
        var jsonOptions = provider.GetRequiredService<IOptions<JsonOptions>>().Value;

        // Assert
        jsonOptions.SerializerOptions.Converters.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void ConfigureRest_ShouldRegisterInvalidFilterExceptionHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        services.ConfigureRest(configuration);

        // Assert
        services.ShouldContain(d => d.ImplementationType == typeof(InvalidFilterExceptionHandler));
    }

    [Fact]
    public void ConfigureRest_ShouldRegisterConflictExceptionHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        services.ConfigureRest(configuration);

        // Assert
        services.ShouldContain(d => d.ImplementationType == typeof(ConflictExceptionHandler));
    }
}

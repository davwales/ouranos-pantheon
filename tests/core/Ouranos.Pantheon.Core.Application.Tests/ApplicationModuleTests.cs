using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Tests.Utils.Shouldly;

namespace Ouranos.Pantheon.Core.Application.Tests;

public sealed class ApplicationModuleTests
{
    [Fact]
    public void AddApplicationModule_ShouldRegisterExpectedServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCoreApplicationModule();

        // Assert
        services.ShouldContainService<IDispatcher>(ServiceLifetime.Transient);
    }
}
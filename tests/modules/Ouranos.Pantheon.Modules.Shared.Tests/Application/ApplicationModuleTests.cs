using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Tests.Utils.Shouldly;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Application;

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
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;

namespace Ouranos.Pantheon.Modules.Hestia.Tests;

public sealed class HestiaModuleTests
{
    [Fact]
    public void Build_WhenInvoked_ShouldRegisterMartenStore()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions { EnvironmentName = "Testing" }
        );

        // Act
        new HestiaModule().Build(builder);
        var host = builder.Build();

        // Assert
        var store = host.Services.GetService<IHestiaMartenStore>();
        store.ShouldNotBeNull();
    }
}

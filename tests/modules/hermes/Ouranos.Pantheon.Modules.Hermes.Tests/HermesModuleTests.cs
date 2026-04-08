using Microsoft.AspNetCore.Builder;

namespace Ouranos.Pantheon.Modules.Hermes.Tests;

public sealed class HermesModuleTests
{
    [Fact]
    public void MapEndpoints_ShouldRegisterAllEndpointsWithoutThrowing()
    {
        // Arrange
        var app = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" }).Build();

        // Act
        var act = () => new HermesModule().MapEndpoints(app);

        // Assert
        act.ShouldNotThrow();
    }

    [Fact]
    public void Build_ShouldRegisterServicesWithoutThrowing()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });

        // Act
        var act = () => new HermesModule().Build(builder);

        // Assert
        act.ShouldNotThrow();
    }
}

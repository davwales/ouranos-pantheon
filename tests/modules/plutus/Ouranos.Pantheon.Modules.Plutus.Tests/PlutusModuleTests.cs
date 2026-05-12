using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests;

public sealed class PlutusModuleTests
{
    [Fact]
    public void MapEndpoints_ShouldRegisterAllEndpointsWithoutThrowing()
    {
        // Arrange
        var app = WebApplication
            .CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" })
            .Build();

        // Act
        var act = () => new PlutusModule().MapEndpoints(app);

        // Assert
        act.ShouldNotThrow();
    }

    [Fact]
    public void Build_ShouldRegisterServicesWithoutThrowing()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions { EnvironmentName = "Testing" }
        );

        // Act
        var act = () => new PlutusModule().Build(builder);

        // Assert
        act.ShouldNotThrow();
    }

    [Fact]
    public void ConfigureWolverine_WhenAllLoadersDisabled_ShouldReturnEarlyWithoutThrowing()
    {
        // Arrange
        var opts = new WolverineOptions();
        var config = new ConfigurationBuilder().Build();

        // Act
        var act = () => new PlutusModule().ConfigureWolverine(opts, config);

        // Assert
        act.ShouldNotThrow();
    }
}

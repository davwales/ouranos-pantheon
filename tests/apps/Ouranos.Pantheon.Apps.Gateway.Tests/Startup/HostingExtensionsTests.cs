using System.Reflection;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Apps.Gateway.Startup;
using Ouranos.Pantheon.Modules.Hermes;
using Ouranos.Pantheon.Modules.Plutus;
using Ouranos.Pantheon.Modules.Shared;

namespace Ouranos.Pantheon.Apps.Gateway.Tests.Startup;

public sealed class HostingExtensionsTests
{
    private static void InvokeConfigureCors(IServiceCollection services, IConfiguration configuration)
    {
        var method = typeof(HostingExtensions).GetMethod(
            "ConfigureCors",
            BindingFlags.NonPublic | BindingFlags.Static
        )!;

        method.Invoke(null, [services, configuration]);
    }

    [Fact]
    public void ConfigureCors_WhenHostsConfigured_ShouldRegisterPolicyWithOrigins()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    { "CorsAllowedHosts:0", "http://localhost:3000" },
                    { "CorsAllowedHosts:1", "http://talos.local" }
                }
            )
            .Build();

        // Act
        InvokeConfigureCors(services, configuration);
        var corsOptions = services.BuildServiceProvider()
            .GetRequiredService<IOptions<CorsOptions>>()
            .Value;

        // Assert
        var policy = corsOptions.GetPolicy("AllowLocalAndServer");
        policy.ShouldNotBeNull();
        policy.Origins.ShouldContain("http://localhost:3000");
        policy.Origins.ShouldContain("http://talos.local");
    }

    [Fact]
    public void ConfigureCors_WhenNoHostsConfigured_ShouldRegisterPolicyWithEmptyOrigins()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        InvokeConfigureCors(services, configuration);
        var corsOptions = services.BuildServiceProvider()
            .GetRequiredService<IOptions<CorsOptions>>()
            .Value;

        // Assert
        var policy = corsOptions.GetPolicy("AllowLocalAndServer");
        policy.ShouldNotBeNull();
        policy.Origins.ShouldBeEmpty();
    }

    [Fact]
    public void Modules_ShouldContainHermesAndPlutus()
    {
        // Arrange
        var field = typeof(HostingExtensions).GetField(
            "Modules",
            BindingFlags.NonPublic | BindingFlags.Static
        )!;

        // Act
        var modules = (IReadOnlyList<IPantheonModule>)field.GetValue(null)!;

        // Assert
        modules.ShouldContain(m => m is HermesModule);
        modules.ShouldContain(m => m is PlutusModule);
    }
}

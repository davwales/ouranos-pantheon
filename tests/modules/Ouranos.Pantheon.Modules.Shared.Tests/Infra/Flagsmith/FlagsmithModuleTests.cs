using Flagsmith;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Shared.Infra.Flagsmith;
using Ouranos.Pantheon.Tests.Utils.Shouldly;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Infra.Flagsmith;

public sealed class FlagsmithModuleTests
{
    [Fact]
    public void AddCoreFlagsmithModule_ShouldRegisterFlagsmithClient()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{FlagsmithOptions.SectionName}:{nameof(FlagsmithOptions.ApiUrl)}"] = "https://edge.api.flagsmith.com/api/v1/",
                [$"{FlagsmithOptions.SectionName}:{nameof(FlagsmithOptions.EnvironmentKey)}"] = "test-key",
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        services.AddCoreFlagsmithModule(configuration);

        // Assert
        services.ShouldContainService<IFlagsmithClient>(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddCoreFlagsmithModule_WhenResolvingClient_ShouldReturnFlagsmithClient()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{FlagsmithOptions.SectionName}:{nameof(FlagsmithOptions.ApiUrl)}"] = "https://edge.api.flagsmith.com/api/v1/",
                [$"{FlagsmithOptions.SectionName}:{nameof(FlagsmithOptions.EnvironmentKey)}"] = "test-key",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddCoreFlagsmithModule(configuration);

        // Act
        var provider = services.BuildServiceProvider();
        var client = provider.GetService<IFlagsmithClient>();

        // Assert
        client.ShouldNotBeNull();
    }
}

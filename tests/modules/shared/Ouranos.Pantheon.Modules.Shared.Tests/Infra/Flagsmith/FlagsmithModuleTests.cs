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
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    [$"{FlagsmithOptions.SectionName}:{nameof(FlagsmithOptions.ApiUrl)}"] =
                        "https://edge.api.flagsmith.com/api/v1/",
                    [$"{FlagsmithOptions.SectionName}:{nameof(FlagsmithOptions.EnvironmentKey)}"] =
                        "test-key",
                }
            )
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
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    [$"{FlagsmithOptions.SectionName}:{nameof(FlagsmithOptions.ApiUrl)}"] =
                        "https://edge.api.flagsmith.com/api/v1/",
                    [$"{FlagsmithOptions.SectionName}:{nameof(FlagsmithOptions.EnvironmentKey)}"] =
                        "test-key",
                }
            )
            .Build();

        var services = new ServiceCollection();
        services.AddCoreFlagsmithModule(configuration);

        // Act
        var provider = services.BuildServiceProvider();
        var client = provider.GetService<IFlagsmithClient>();

        // Assert
        client.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetEnvironmentFlags_WhenApiUnreachable_ShouldReturnDefaultFlags()
    {
        // Arrange
        var client = CreateUnreachableClient();

        // Act
        var flags = await client.GetEnvironmentFlags();

        // Assert
        flags.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetEnvironmentFlags_WhenApiUnreachable_ShouldEnableConfiguredDefaultFlag()
    {
        // Arrange
        var client = CreateUnreachableClient();
        var flags = await client.GetEnvironmentFlags();

        // Act
        var isEnabled = await flags.IsFeatureEnabled("test.default_on");

        // Assert
        isEnabled.ShouldBeTrue();
    }

    [Fact]
    public async Task GetEnvironmentFlags_WhenApiUnreachableAndFlagUnknown_ShouldDisableFlag()
    {
        // Arrange
        var client = CreateUnreachableClient();
        var flags = await client.GetEnvironmentFlags();

        // Act
        var isEnabled = await flags.IsFeatureEnabled("test.unknown");

        // Assert
        isEnabled.ShouldBeFalse();
    }

    [Fact]
    public async Task GetEnvironmentFlags_WhenApiUnreachableAndDefaultFlagFalse_ShouldDisableFlag()
    {
        // Arrange
        var client = CreateUnreachableClient();
        var flags = await client.GetEnvironmentFlags();

        // Act
        var isEnabled = await flags.IsFeatureEnabled("test.default_off");

        // Assert
        isEnabled.ShouldBeFalse();
    }

    [Fact]
    public async Task GetEnvironmentFlags_WhenCalledTwice_ShouldReturnCachedFlags()
    {
        // Arrange
        var client = CreateUnreachableClient();
        var first = await client.GetEnvironmentFlags();

        // Act
        var second = await client.GetEnvironmentFlags();

        // Assert
        second.ShouldBeSameAs(first);
    }

    private static IFlagsmithClient CreateUnreachableClient()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    [$"{FlagsmithOptions.SectionName}:{nameof(FlagsmithOptions.ApiUrl)}"] =
                        "http://127.0.0.1:1/api/v1/",
                    [$"{FlagsmithOptions.SectionName}:{nameof(FlagsmithOptions.EnvironmentKey)}"] =
                        "test-key",
                    [$"{FlagsmithOptions.SectionName}:{nameof(FlagsmithOptions.Retries)}"] = "0",
                    [
                        $"{FlagsmithOptions.SectionName}:{nameof(FlagsmithOptions.DefaultFlags)}:test.default_on"
                    ] = "true",
                    [
                        $"{FlagsmithOptions.SectionName}:{nameof(FlagsmithOptions.DefaultFlags)}:test.default_off"
                    ] = "false",
                }
            )
            .Build();

        var services = new ServiceCollection();
        services.AddCoreFlagsmithModule(configuration);

        return services.BuildServiceProvider().GetRequiredService<IFlagsmithClient>();
    }
}

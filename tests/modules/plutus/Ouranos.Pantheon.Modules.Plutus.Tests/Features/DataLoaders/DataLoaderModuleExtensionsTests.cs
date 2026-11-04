using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Shared;
using Ouranos.Pantheon.Tests.Utils.Shouldly;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.DataLoaders;

public sealed class DataLoaderModuleExtensionsTests
{
    private const string LoadersKey = "Ouranos:Plutus:DataLoaders";

    private static Dictionary<string, string?> Loaders(bool isEnabled)
    {
        return Loaders(isEnabled, isEnabled);
    }

    private static Dictionary<string, string?> Loaders(bool ffxivEnabled, bool stocksEnabled)
    {
        var othersEnabled = (ffxivEnabled || stocksEnabled).ToString();
        return new Dictionary<string, string?>
        {
            [$"{LoadersKey}:Ffxiv:IsEnabled"] = ffxivEnabled.ToString(),
            [$"{LoadersKey}:Osrs:IsEnabled"] = othersEnabled,
            [$"{LoadersKey}:Stocks:IsEnabled"] = stocksEnabled.ToString(),
            [$"{LoadersKey}:Consumer:IsEnabled"] = othersEnabled,
        };
    }

    private static HostApplicationBuilder CreateBuilder(Dictionary<string, string?> loaders)
    {
        var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());
        builder.Configuration.AddInMemoryCollection(loaders);
        return builder;
    }

    private static int CountHostedServices(IServiceCollection services)
    {
        return services.Count(d => d.ServiceType == typeof(IHostedService));
    }

    [Fact]
    public void AddDataLoaders_ShouldRegisterTheLoaderServices()
    {
        // Arrange
        var builder = CreateBuilder(Loaders(isEnabled: false));

        // Act
        builder.AddDataLoaders();

        // Assert
        builder.Services.ShouldContainService<IQueueTradeMessages>(ServiceLifetime.Singleton);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void AddDataLoaders_WhenOneWebSocketLoaderIsEnabled_ShouldRegisterItsWorker(
        bool ffxivEnabled,
        bool stocksEnabled
    )
    {
        // Arrange
        var builder = CreateBuilder(Loaders(ffxivEnabled, stocksEnabled));
        var before = CountHostedServices(builder.Services);

        // Act
        builder.AddDataLoaders();

        // Assert
        (CountHostedServices(builder.Services) - before).ShouldBe(1);
    }

    [Fact]
    public void AddDataLoaders_WhenWebSocketLoadersAreDisabled_ShouldRegisterNoWorkers()
    {
        // Arrange
        var builder = CreateBuilder(Loaders(isEnabled: false));
        var before = CountHostedServices(builder.Services);

        // Act
        builder.AddDataLoaders();

        // Assert
        (CountHostedServices(builder.Services) - before).ShouldBe(0);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ConfigureDataLoaderMessaging_ShouldConfigureWithoutThrowing(bool loadersEnabled)
    {
        // Arrange
        var opts = new WolverineOptions();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(Loaders(loadersEnabled))
            .Build();

        // Act
        var act = () => opts.ConfigureDataLoaderMessaging(configuration);

        // Assert
        act.ShouldNotThrow();
    }
}

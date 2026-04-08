using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Tests.Utils.Shouldly;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Infra.OuranosMachineLearning;

public sealed class OuranosMachineLearningModuleTests
{
    [Fact]
    public void AddCoreOuranosMachineLearningModule_ShouldAddExpectedServices()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();

        // Act
        services.AddCoreOuranosMachineLearningModule(configuration);

        // Assert
        services.ShouldContainService<IOuranosMachineLearningClient>(ServiceLifetime.Transient);
    }

    [Fact]
    public void AddCoreOuranosMachineLearningModule_WhenConnectionStringProvided_ShouldBuildClient()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        services.AddLogging();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    [$"{OuranosMachineLearningOptions.SectionName}:ConnectionString"] = "http://localhost:5000/",
                    [$"{OuranosMachineLearningOptions.SectionName}:ApiKey"] = "test-api-key",
                }
            )
            .Build();

        services.AddCoreOuranosMachineLearningModule(configuration);

        var sp = services.BuildServiceProvider();

        // Act
        var client = sp.GetRequiredService<IOuranosMachineLearningClient>();

        // Assert
        client.ShouldNotBeNull();
    }

    [Fact]
    public void AddCoreOuranosMachineLearningModule_WhenConnectionStringEmpty_ShouldThrowOnResolve()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        services.AddLogging();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    [$"{OuranosMachineLearningOptions.SectionName}:ConnectionString"] = "",
                }
            )
            .Build();

        services.AddCoreOuranosMachineLearningModule(configuration);

        var sp = services.BuildServiceProvider();

        // Act
        var act = () => sp.GetRequiredService<IOuranosMachineLearningClient>();

        // Assert
        act.ShouldThrow<Exception>();
    }
}
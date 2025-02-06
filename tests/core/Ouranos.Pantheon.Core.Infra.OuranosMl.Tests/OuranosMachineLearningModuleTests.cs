using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Tests.Utils.Shouldly;

namespace Ouranos.Pantheon.Core.Infra.OuranosMl.Tests;

public sealed class OuranosMachineLearningModuleTests
{
    [Fact]
    public void AddCoreOuranosMachineLearningModule_ShouldAddExpectedServices()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        var mockConfiguration = new Mock<IConfiguration>();

        // Act
        services.AddCoreOuranosMachineLearningModule(mockConfiguration.Object);

        // Assert
        services.ShouldContainService(typeof(IOuranosMachineLearningClient), ServiceLifetime.Transient);
    }
}
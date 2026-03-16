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
}
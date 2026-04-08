using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Infra.Flagsmith;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Infra.RabbitMq;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Infra;

public sealed class OptionsTests
{
    [Fact]
    public void FlagsmithOptions_DefaultConstructor_ShouldHaveEmptyDefaults()
    {
        // Act
        var options = new FlagsmithOptions();

        // Assert
        options.ApiUrl.ShouldBe(string.Empty);
        options.EnvironmentKey.ShouldBe(string.Empty);
    }

    [Fact]
    public void FlagsmithOptions_WhenConstructed_ShouldSetValues()
    {
        // Act
        var options = new FlagsmithOptions("https://api.flagsmith.com", "test-key");

        // Assert
        options.ApiUrl.ShouldBe("https://api.flagsmith.com");
        options.EnvironmentKey.ShouldBe("test-key");
    }

    [Fact]
    public void OuranosMachineLearningOptions_DefaultConstructor_ShouldHaveDefaults()
    {
        // Act
        var options = new OuranosMachineLearningOptions();

        // Assert
        options.ConnectionString.ShouldBe(string.Empty);
        options.ApiKey.ShouldBe("no-key");
    }

    [Fact]
    public void RabbitMqOptions_DefaultConstructor_ShouldHaveEmptyDefaults()
    {
        // Act
        var options = new RabbitMqOptions();

        // Assert
        options.Host.ShouldBe(string.Empty);
        options.Username.ShouldBe(string.Empty);
        options.Password.ShouldBe(string.Empty);
        options.RetryCount.ShouldBeNull();
    }

    [Fact]
    public void QueryOptions_DefaultConstructor_ShouldSetSensibleDefaults()
    {
        // Act
        var options = new QueryOptions();

        // Assert
        options.MinPageSize.ShouldBe(1);
        options.MaxPageSize.ShouldBe(100);
        options.MaxSkip.ShouldBe(10000);
        QueryOptions.SectionName.ShouldBe("Ouranos:Query");
    }

    [Fact]
    public void QueryOptions_WhenConstructedWithValues_ShouldSetProperties()
    {
        // Act
        var options = new QueryOptions(MinPageSize: 5, MaxPageSize: 50, MaxSkip: 500);

        // Assert
        options.MinPageSize.ShouldBe(5);
        options.MaxPageSize.ShouldBe(50);
        options.MaxSkip.ShouldBe(500);
    }
}

using Microsoft.Extensions.Configuration;
using Ouranos.Pantheon.Modules.Hestia.Shared;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Shared;

public sealed class RecipeImportOptionsTests
{
    [Fact]
    public void StaleImportTimeout_ShouldDefaultToFifteenMinutes()
    {
        // Act
        var options = new RecipeImportOptions();

        // Assert
        options.StaleImportTimeout.ShouldBe(TimeSpan.FromMinutes(15));
    }

    [Fact]
    public void StaleImportTimeout_WhenConfigured_ShouldBindFromConfiguration()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Ouranos:Hestia:RecipeImport:StaleImportTimeout"] = "00:30:00",
                }
            )
            .Build();

        // Act
        var options = configuration.GetSection(HestiaOptions.SectionName).Get<HestiaOptions>();

        // Assert
        options
            .ShouldNotBeNull()
            .RecipeImport.StaleImportTimeout.ShouldBe(TimeSpan.FromMinutes(30));
    }
}

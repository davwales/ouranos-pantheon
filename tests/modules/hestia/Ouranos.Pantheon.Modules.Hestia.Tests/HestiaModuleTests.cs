using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Extraction;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Scraping;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Tests.Utils.Shouldly;

namespace Ouranos.Pantheon.Modules.Hestia.Tests;

public sealed class HestiaModuleTests
{
    [Fact]
    public void Build_WhenInvoked_ShouldRegisterMartenStore()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions { EnvironmentName = "Testing" }
        );

        // Act
        new HestiaModule().Build(builder);
        var host = builder.Build();

        // Assert
        var store = host.Services.GetService<IHestiaMartenStore>();
        store.ShouldNotBeNull();
    }

    [Fact]
    public void Build_WhenInvoked_ShouldRegisterRecipeScraper()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions { EnvironmentName = "Testing" }
        );

        // Act
        new HestiaModule().Build(builder);

        // Assert
        builder.Services.ShouldContainService<IRecipeScraper>(ServiceLifetime.Transient);
    }

    [Fact]
    public void Build_WhenInvoked_ShouldRegisterRecipeExtractor()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions { EnvironmentName = "Testing" }
        );

        // Act
        new HestiaModule().Build(builder);

        // Assert
        builder.Services.ShouldContainServiceWithImplementation<IRecipeExtractor, RecipeExtractor>(
            ServiceLifetime.Scoped
        );
    }
}

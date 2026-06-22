using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Shared.Domain.Recipes;

public sealed class RecipeDocumentTests
{
    [Fact]
    public void RecipeId_WhenIdIsSet_ShouldRoundTripGuidAsString()
    {
        // Arrange
        var id = Guid.NewGuid();
        var document = new RecipeDocument { Id = id };

        // Act
        var recipeId = document.RecipeId;

        // Assert
        recipeId.ShouldBe(new Id<RecipeDocument>(id.ToString()));
        Guid.Parse(recipeId.Value).ShouldBe(id);
    }
}

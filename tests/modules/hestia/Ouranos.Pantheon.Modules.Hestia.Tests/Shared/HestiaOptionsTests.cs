using Ouranos.Pantheon.Modules.Hestia.Shared;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Shared;

public sealed class HestiaOptionsTests
{
    [Fact]
    public void KitchenAssistant_ShouldDefaultToGlobalModelDefaults()
    {
        // Act
        var options = new HestiaOptions().KitchenAssistant;

        // Assert
        options.ModelName.ShouldBe(ModelDefaults.ModelName);
        options.MaxTokens.ShouldBe(ModelDefaults.MaxTokens);
        options.ReasoningEffort.ShouldBe(ModelDefaults.ReasoningEffort);
    }

    [Fact]
    public void RecipeImport_ShouldUseGlobalModelAndOverrideReasoningEffortToLow()
    {
        // Act
        var options = new HestiaOptions().RecipeImport;

        // Assert
        options.ModelName.ShouldBe(ModelDefaults.ModelName);
        options.ReasoningEffort.ShouldBe(ReasoningEffort.Low);
    }
}

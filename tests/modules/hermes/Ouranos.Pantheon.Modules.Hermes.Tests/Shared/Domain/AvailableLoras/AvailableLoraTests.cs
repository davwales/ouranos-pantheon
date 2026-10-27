using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.AvailableLoras;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Shared.Domain.AvailableLoras;

public sealed class AvailableLoraTests
{
    private static AvailableLora Create(string identifier = "detail")
    {
        return AvailableLora.Create(
            new Id<AvailableLora>(Guid.NewGuid().ToString()),
            identifier,
            "Detail",
            "flux-1",
            0.7f,
            "detailed",
            "Use 0.7",
            isCompatible: true
        );
    }

    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        // Act
        var lora = Create();

        // Assert
        lora.Identifier.ShouldBe("detail");
        lora.Title.ShouldBe("Detail");
        lora.CompatClass.ShouldBe("flux-1");
        lora.DefaultWeight.ShouldBe(0.7f);
        lora.TriggerPhrase.ShouldBe("detailed");
        lora.UsageHint.ShouldBe("Use 0.7");
        lora.IsCompatible.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WhenIdentifierIsBlank_ShouldThrowArgumentException(string identifier)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => Create(identifier));
    }

    [Fact]
    public void Update_ShouldReplaceTheDetailsButKeepTheIdentifier()
    {
        // Arrange
        var lora = Create();

        // Act
        lora.Update(null, "sdxl-1", null, null, null, isCompatible: false);

        // Assert
        lora.Identifier.ShouldBe("detail");
        lora.Title.ShouldBeNull();
        lora.CompatClass.ShouldBe("sdxl-1");
        lora.DefaultWeight.ShouldBeNull();
        lora.TriggerPhrase.ShouldBeNull();
        lora.UsageHint.ShouldBeNull();
        lora.IsCompatible.ShouldBeFalse();
    }
}

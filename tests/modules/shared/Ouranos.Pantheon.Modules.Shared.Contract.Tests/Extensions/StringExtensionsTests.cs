using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Extensions;

public sealed class StringExtensionsTests
{
    [Theory]
    [InlineData("short", 10, "short")]
    [InlineData("exactly10!", 10, "exactly10!")]
    [InlineData("much too long", 4, "much…")]
    public void TruncateWithEllipsis_ShouldCutOnlyValuesThatDoNotFit(
        string value,
        int maxLength,
        string expected
    )
    {
        // Act
        var result = value.TruncateWithEllipsis(maxLength);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("short", 10, "short")]
    [InlineData("A red fox", 5, "A red")]
    [InlineData("A red fox", 6, "A red")]
    [InlineData("A red fox", 4, "A")]
    [InlineData("A fox, in snow", 8, "A fox")]
    [InlineData("A fox -- in snow", 9, "A fox")]
    [InlineData("A fox (in snow)", 7, "A fox")]
    [InlineData("A fox.\nIn snow", 8, "A fox.")]
    [InlineData("A fox <lora:pixel-art:0.8> in snow", 22, "A fox")]
    [InlineData("A fox <lora:pixel art:0.8> in snow", 18, "A fox")]
    [InlineData("A fox <b>bold</b> in snow", 18, "A fox <b>bold</b>")]
    [InlineData("Supercalifragilistic", 5, "Super")]
    public void TruncateAtWordBoundary_ShouldCutOnAWordBoundaryWithoutDanglingText(
        string value,
        int maxLength,
        string expected
    )
    {
        // Act
        var result = value.TruncateAtWordBoundary(maxLength);

        // Assert
        result.ShouldBe(expected);
    }
}

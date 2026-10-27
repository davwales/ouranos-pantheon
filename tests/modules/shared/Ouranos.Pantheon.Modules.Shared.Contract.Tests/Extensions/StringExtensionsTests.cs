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
}

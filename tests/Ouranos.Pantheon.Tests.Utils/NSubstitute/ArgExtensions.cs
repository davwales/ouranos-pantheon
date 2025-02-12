using NSubstitute.Core.Arguments;

namespace Ouranos.Pantheon.Tests.Utils.NSubstitute;

public static class ArgExtensions
{
    public static ref T IsEquivalent<T>(T? expected)
    {
        return ref ArgumentMatcher.Enqueue(new EquivalentArgumentMatcher<T>(expected))!;
    }
}
using Shouldly;

namespace Ouranos.Pantheon.Tests.Utils.Shouldly;

[ShouldlyMethods]
public static class AsyncEnumerableValidations
{
    public static async Task ShouldMatchAsync<T>(
        this IAsyncEnumerable<T> actual,
        IReadOnlyList<T> expected,
        Action<T, T>? compare = null
    )
    {
        List<T> actualList = [];
        await foreach (var element in actual)
        {
            actualList.Add(element);
        }

        actualList.Count.ShouldBe(expected.Count);
        for (var i = 0; i < actualList.Count; i++)
        {
            if (compare is null)
            {
                actualList[i].ShouldBe(expected[i]);
            }
            else
            {
                compare(expected[i], actualList[i]);
            }
        }
    }
}

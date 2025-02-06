namespace Ouranos.Pantheon.Tests.Utils.Extensions;

public static class EnumerableExtensions
{
    public static async IAsyncEnumerable<TResult> ToAsyncEnumerable<TResult>(this IEnumerable<TResult> source)
    {
        foreach (var x in source)
        {
            yield return await Task.FromResult(x);
        }
    }
}
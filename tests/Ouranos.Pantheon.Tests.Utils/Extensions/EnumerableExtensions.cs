using System.Text;

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

    public static MemoryStream AsMemoryStream(this IEnumerable<string> input)
    {
        var stream = new MemoryStream(input.SelectMany(x => Encoding.UTF8.GetBytes(x)).ToArray());
        stream.Position = 0;
        return stream;
    }
}
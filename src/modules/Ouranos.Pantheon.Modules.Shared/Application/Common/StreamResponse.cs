using System.Runtime.CompilerServices;

namespace Ouranos.Pantheon.Modules.Shared.Application.Common;

public sealed class StreamResponse<TSource, TResult>(
    Func<CancellationToken, Task<IAsyncEnumerable<TSource>>> streamFactory,
    Func<TSource, Task<TResult>> transform
)
{
    public async IAsyncEnumerable<TResult> GetStream(
        [EnumeratorCancellation] CancellationToken token = default
    )
    {
        var stream = await streamFactory(token);
        await foreach (var item in stream.WithCancellation(token))
        {
            yield return await transform(item);
        }
    }
}

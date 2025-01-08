namespace Ouranos.Pantheon.Core.API.Models;

public sealed record StreamResponse<T>(
    [property: StreamResult] IAsyncEnumerable<T> Chunks
);
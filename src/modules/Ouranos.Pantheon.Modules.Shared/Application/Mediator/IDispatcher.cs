using Ouranos.Pantheon.Modules.Shared.Application.Common;

namespace Ouranos.Pantheon.Modules.Shared.Application.Mediator;

public interface IDispatcher
{
    Task Send(IRequest request, CancellationToken cancellationToken = default);

    Task<TResult> Send<TResult>(
        IRequest<TResult> request,
        CancellationToken cancellationToken = default
    ) where TResult : class;

    IAsyncEnumerable<TResult> CreateStream<TSource, TResult>(
        IRequest<StreamResponse<TSource, TResult>> request,
        CancellationToken cancellationToken = default
    );
}
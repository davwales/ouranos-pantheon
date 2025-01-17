using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Mediator;

namespace Ouranos.Pantheon.Core.Application.Mediator;

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
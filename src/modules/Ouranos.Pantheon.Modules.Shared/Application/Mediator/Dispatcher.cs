using System.Runtime.CompilerServices;
using Ardalis.GuardClauses;
using MassTransit.Mediator;
using Ouranos.Pantheon.Modules.Shared.Application.Common;

namespace Ouranos.Pantheon.Modules.Shared.Application.Mediator;

public sealed class Dispatcher<TMediator> : IScopedDispatcher where TMediator : IMediator
{
    private readonly TMediator _mediator;

    public Dispatcher(TMediator mediator)
    {
        Guard.Against.Null(mediator);
        _mediator = mediator;
    }

    public async Task Send(IRequest request, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(request, cancellationToken);
    }

    public async Task<TResult> Send<TResult>(
        IRequest<TResult> request,
        CancellationToken cancellationToken = default
    ) where TResult : class
    {
        var requestClient = _mediator.CreateRequestClient<IRequest<TResult>>();
        var response = await requestClient.GetResponse<TResult>(request, cancellationToken);
        return response.Message;
    }

    public async IAsyncEnumerable<TResult> CreateStream<TSource, TResult>(
        IRequest<StreamResponse<TSource, TResult>> request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var requestClient = _mediator.CreateRequestClient<IRequest<StreamResponse<TSource, TResult>>>();
        var response = await requestClient.GetResponse<StreamResponse<TSource, TResult>>(request, cancellationToken);

        await foreach (var item in response.Message.GetStream(cancellationToken))
        {
            yield return item;
        }
    }
}
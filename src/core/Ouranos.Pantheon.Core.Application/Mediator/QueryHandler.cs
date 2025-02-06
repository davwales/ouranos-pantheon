using MassTransit;

namespace Ouranos.Pantheon.Core.Application.Mediator;

public abstract class QueryHandler<TInput, TOutput> : IConsumer<TInput>
    where TInput : class, IQuery<TOutput>
    where TOutput : class
{
    public async Task Consume(ConsumeContext<TInput> context)
    {
        var result = await Handle(context.Message, context.CancellationToken);
        await context.RespondAsync(result);
    }

    public abstract Task<TOutput> Handle(TInput query, CancellationToken cancellationToken = default);
}
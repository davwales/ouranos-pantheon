using MassTransit;

namespace Ouranos.Pantheon.Modules.Shared.Application.Mediator;

public abstract class CommandHandler<TInput, TOutput> : IConsumer<TInput>
    where TInput : class, ICommand<TOutput>
    where TOutput : class
{
    public async Task Consume(ConsumeContext<TInput> context)
    {
        var result = await Handle(context.Message, context.CancellationToken);
        await context.RespondAsync(result);
    }

    public abstract Task<TOutput> Handle(TInput command, CancellationToken cancellationToken = default);
}

public abstract class CommandHandler<TInput> : IConsumer<TInput> where TInput : class, ICommand
{
    public async Task Consume(ConsumeContext<TInput> context)
    {
        await Handle(context.Message, context.CancellationToken);
    }

    public abstract Task Handle(TInput command, CancellationToken cancellationToken = default);
}
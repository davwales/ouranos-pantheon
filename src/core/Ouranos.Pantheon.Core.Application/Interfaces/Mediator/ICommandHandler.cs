using MassTransit;

namespace Ouranos.Pantheon.Core.Application.Interfaces.Mediator;

public interface ICommandHandler<in TInput, TOutput> : IConsumer<TInput>
    where TInput : class, ICommand<TOutput> where TOutput : class;

public interface ICommandHandler<in TInput> : IConsumer<TInput> where TInput : class, ICommand;
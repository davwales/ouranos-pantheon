using MassTransit;

namespace Ouranos.Pantheon.Core.Application.Interfaces.Mediator;

public interface IQueryHandler<in TInput, TOutput> : IConsumer<TInput>
    where TInput : class, IQuery<TOutput> where TOutput : class;
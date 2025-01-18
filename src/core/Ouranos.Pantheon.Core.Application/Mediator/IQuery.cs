namespace Ouranos.Pantheon.Core.Application.Mediator;

public interface IQuery<TOutput> : IRequest<TOutput> where TOutput : class;
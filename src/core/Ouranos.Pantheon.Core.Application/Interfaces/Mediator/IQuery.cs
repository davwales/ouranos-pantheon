namespace Ouranos.Pantheon.Core.Application.Interfaces.Mediator;

public interface IQuery<TOutput> : IRequest<TOutput> where TOutput : class;
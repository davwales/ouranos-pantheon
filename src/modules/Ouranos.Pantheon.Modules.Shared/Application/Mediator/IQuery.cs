namespace Ouranos.Pantheon.Modules.Shared.Application.Mediator;

public interface IQuery<TOutput> : IRequest<TOutput> where TOutput : class;
namespace Ouranos.Pantheon.Core.Application.Interfaces.Mediator;

public interface ICommand<TOutput> : IRequest<TOutput> where TOutput : class;

public interface ICommand : IRequest;
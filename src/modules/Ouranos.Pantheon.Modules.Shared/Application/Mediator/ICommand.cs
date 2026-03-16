namespace Ouranos.Pantheon.Modules.Shared.Application.Mediator;

public interface ICommand<TOutput> : IRequest<TOutput> where TOutput : class;

public interface ICommand : IRequest;
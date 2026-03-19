namespace Ouranos.Pantheon.Modules.Shared.Application;

public interface IPantheonHandler { }

public interface IPantheonHandler<TInput, TOutput> : IPantheonHandler
    where TInput : class
    where TOutput : class
{
    Task<TOutput> Handle(TInput input, CancellationToken cancellationToken = default);
}

public interface IPantheonHandler<TInput> : IPantheonHandler
    where TInput : class
{
    Task Handle(TInput input, CancellationToken cancellationToken = default);
}

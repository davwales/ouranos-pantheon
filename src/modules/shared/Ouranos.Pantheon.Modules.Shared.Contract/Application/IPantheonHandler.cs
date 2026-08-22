namespace Ouranos.Pantheon.Modules.Shared.Contract.Application;

public interface IPantheonHandler { }

public interface IPantheonHandler<in TInput, TOutput> : IPantheonHandler
    where TInput : class
    where TOutput : class
{
    Task<TOutput> Handle(TInput input, CancellationToken cancellationToken = default);
}

public interface IPantheonHandler<in TInput> : IPantheonHandler
    where TInput : class
{
    Task Handle(TInput input, CancellationToken cancellationToken = default);
}

public interface IPantheonStreamHandler<in TInput, out TOutput> : IPantheonHandler
    where TInput : class
    where TOutput : class
{
    IAsyncEnumerable<TOutput> Handle(TInput input, CancellationToken cancellationToken = default);
}

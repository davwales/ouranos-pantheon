namespace Ouranos.Pantheon.Modules.Shared.Contract.Application;

public interface IPantheonStreamHandler<in TInput, out TOutput> : IPantheonHandler
    where TInput : class
    where TOutput : class
{
    IAsyncEnumerable<TOutput> Handle(TInput input, CancellationToken cancellationToken = default);
}

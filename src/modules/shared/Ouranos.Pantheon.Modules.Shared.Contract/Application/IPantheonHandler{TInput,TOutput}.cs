namespace Ouranos.Pantheon.Modules.Shared.Contract.Application;

public interface IPantheonHandler<in TInput, TOutput> : IPantheonHandler
    where TInput : class
    where TOutput : class
{
    Task<TOutput> Handle(TInput input, CancellationToken cancellationToken = default);
}

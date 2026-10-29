namespace Ouranos.Pantheon.Modules.Shared.Contract.Application;

public interface IPantheonHandler<in TInput> : IPantheonHandler
    where TInput : class
{
    Task Handle(TInput input, CancellationToken cancellationToken = default);
}

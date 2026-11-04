namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;

public interface ISignalComputer
{
    SignalType Type { get; }

    string Label { get; }

    string Description { get; }

    IReadOnlyList<InvestmentIntent> Intents { get; }

    /// <summary>Returns null if insufficient data.</summary>
    decimal? Compute(SignalComputeContext context);
}

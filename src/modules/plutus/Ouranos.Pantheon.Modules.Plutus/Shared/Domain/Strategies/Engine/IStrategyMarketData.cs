namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Engine;

/// <summary>
///     Market history loaded once by an <see cref="IStrategyEngine" /> and simulated against
///     many times, e.g. once per optimization candidate.
/// </summary>
public interface IStrategyMarketData;

using Ardalis.GuardClauses;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization;

/// <summary>
///     The numbers an optimizer may change in a configuration. Fields that are not parameters
///     are carried over from the seed by the build function.
/// </summary>
public sealed class ParameterSpace
{
    private readonly Func<IReadOnlyList<double>, StrategyConfiguration> _build;

    public ParameterSpace(
        IReadOnlyList<Parameter> parameters,
        IReadOnlyList<double> seed,
        Func<IReadOnlyList<double>, StrategyConfiguration> build
    )
    {
        Guard.Against.Null(parameters);
        Guard.Against.Null(seed);
        Guard.Against.Null(build);
        Guard.Against.InvalidInput(
            seed,
            nameof(seed),
            s => s.Count == parameters.Count,
            "Seed must have one value per parameter."
        );

        Parameters = parameters;
        Seed = [.. seed.Select((value, i) => parameters[i].Clamp(value))];
        _build = build;
    }

    public IReadOnlyList<Parameter> Parameters { get; }

    public IReadOnlyList<double> Seed { get; }

    public StrategyConfiguration Build(IReadOnlyList<double> values)
    {
        Guard.Against.InvalidInput(
            values,
            nameof(values),
            v => v.Count == Parameters.Count,
            "Expected one value per parameter."
        );

        return _build(values);
    }

    public static decimal ToDecimal(double value)
    {
        return Math.Round((decimal)value, 6);
    }
}

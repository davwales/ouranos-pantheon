using Ardalis.GuardClauses;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization;

/// <summary>
///     The numbers an optimizer may change in a configuration. Fields that are not parameters
///     are carried over from the seed by the build function.
/// </summary>
public sealed class ParameterSpace
{
    private readonly Func<IReadOnlyList<double>, StrategyConfiguration> _build;
    private readonly IReadOnlyList<double> _unclampedSeed;

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
        _unclampedSeed = seed;
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

    /// <summary>
    ///     Pins each named parameter to its seed value, even one outside the parameter's usual
    ///     bounds, so the optimizer keeps it exactly as configured. Built configurations remember
    ///     the locks.
    /// </summary>
    public ParameterSpace Lock(IReadOnlyList<string> keys)
    {
        Guard.Against.Null(keys);

        if (keys.Count == 0)
        {
            return this;
        }

        var known = Parameters.Select(p => p.Key).ToHashSet();
        var unknown = keys.Where(key => !known.Contains(key)).ToList();
        if (unknown.Count > 0)
        {
            throw new ArgumentException(
                $"Unknown locked parameters: {string.Join(", ", unknown)}.",
                nameof(keys)
            );
        }

        var parameters = Parameters
            .Select(
                (p, i) =>
                    keys.Contains(p.Key)
                        ? p with
                        {
                            Min = _unclampedSeed[i],
                            Max = _unclampedSeed[i],
                        }
                        : p
            )
            .ToList();

        if (parameters.TrueForAll(p => p.IsLocked))
        {
            throw new ArgumentException(
                "Every parameter is locked, so there is nothing left to optimize.",
                nameof(keys)
            );
        }

        return new ParameterSpace(
            parameters,
            _unclampedSeed,
            values => _build(values) with { LockedParameters = keys }
        );
    }

    public static decimal ToDecimal(double value)
    {
        return Math.Round((decimal)value, 6);
    }
}

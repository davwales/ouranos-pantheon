using Ardalis.GuardClauses;
using OpenTelemetry.Trace;

namespace Ouranos.Pantheon.Modules.Shared.Infra.Observability;

/// <summary>
/// Sampler that drops root activities (no parent context) whose name matches an
/// exclusion, so background infrastructure work does not start new traces. A matching
/// span nested under a recorded parent is never dropped and is delegated to the inner
/// sampler. Exclusions are exact activity names; a trailing '*' matches by name prefix.
/// </summary>
public sealed class RootSpanExclusionSampler : Sampler
{
    private readonly Sampler _inner;
    private readonly string[] _excludedNames;
    private readonly string[] _excludedNamePrefixes;

    public RootSpanExclusionSampler(IReadOnlyList<string> excludedRootSpanNames, Sampler inner)
    {
        Guard.Against.Null(excludedRootSpanNames);
        Guard.Against.Null(inner);

        foreach (var name in excludedRootSpanNames)
        {
            Guard.Against.NullOrWhiteSpace(name);
        }

        _inner = inner;
        _excludedNamePrefixes =
        [
            .. excludedRootSpanNames
                .Where(name => name.EndsWith('*'))
                .Select(name => name.TrimEnd('*')),
        ];
        _excludedNames = [.. excludedRootSpanNames.Where(name => !name.EndsWith('*'))];
        Description =
            $"RootSpanExclusionSampler({string.Join(", ", excludedRootSpanNames)}) -> {inner.Description}";
    }

    public override SamplingResult ShouldSample(in SamplingParameters samplingParameters)
    {
        if (samplingParameters.ParentContext == default && IsExcluded(samplingParameters.Name))
        {
            return new SamplingResult(SamplingDecision.Drop);
        }

        return _inner.ShouldSample(in samplingParameters);
    }

    private bool IsExcluded(string name)
    {
        if (_excludedNames.Contains(name))
        {
            return true;
        }

        foreach (var prefix in _excludedNamePrefixes)
        {
            if (name.StartsWith(prefix, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}

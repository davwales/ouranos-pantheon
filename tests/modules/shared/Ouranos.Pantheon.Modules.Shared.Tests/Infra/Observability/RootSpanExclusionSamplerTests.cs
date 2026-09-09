using System.Diagnostics;
using OpenTelemetry.Trace;
using Ouranos.Pantheon.Modules.Shared.Infra.Observability;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Infra.Observability;

public sealed class RootSpanExclusionSamplerTests
{
    private readonly Sampler _innerSampler = new AlwaysOnSampler();

    [Fact]
    public void ShouldSample_WhenRootSpanNameExcluded_ShouldDrop()
    {
        // Arrange
        var sampler = new RootSpanExclusionSampler(["postgresql"], _innerSampler);
        var parameters = CreateParameters("postgresql");

        // Act
        var result = sampler.ShouldSample(parameters);

        // Assert
        result.Decision.ShouldBe(SamplingDecision.Drop);
    }

    [Fact]
    public void ShouldSample_WhenRootSpanNameMatchesPrefixExclusion_ShouldDrop()
    {
        // Arrange
        var sampler = new RootSpanExclusionSampler(["CONNECT *"], _innerSampler);
        var parameters = CreateParameters("CONNECT pantheon");

        // Act
        var result = sampler.ShouldSample(parameters);

        // Assert
        result.Decision.ShouldBe(SamplingDecision.Drop);
    }

    [Fact]
    public void ShouldSample_WhenSpanNameOnlyContainsPrefixMidString_ShouldDelegateToInner()
    {
        // Arrange
        var sampler = new RootSpanExclusionSampler(["CONNECT *"], _innerSampler);
        var parameters = CreateParameters("reconnect pantheon");

        // Act
        var result = sampler.ShouldSample(parameters);

        // Assert
        result.Decision.ShouldBe(SamplingDecision.RecordAndSample);
    }

    [Fact]
    public void ShouldSample_WhenExcludedNameHasRecordedParent_ShouldDelegateToInner()
    {
        // Arrange
        var sampler = new RootSpanExclusionSampler(["postgresql"], _innerSampler);
        var parameters = CreateParameters("postgresql", CreateParentContext());

        // Act
        var result = sampler.ShouldSample(parameters);

        // Assert
        result.Decision.ShouldBe(SamplingDecision.RecordAndSample);
    }

    [Fact]
    public void ShouldSample_WhenRootSpanNameNotExcluded_ShouldDelegateToInner()
    {
        // Arrange
        var sampler = new RootSpanExclusionSampler(["wolverine_node_assignments"], _innerSampler);
        var parameters = CreateParameters(
            "Ouranos.Pantheon.Modules.Plutus.Features.Trades.TradeMessage"
        );

        // Act
        var result = sampler.ShouldSample(parameters);

        // Assert
        result.Decision.ShouldBe(SamplingDecision.RecordAndSample);
    }

    [Fact]
    public void ShouldSample_WhenNoExclusions_ShouldDelegateToInner()
    {
        // Arrange
        var sampler = new RootSpanExclusionSampler([], _innerSampler);
        var parameters = CreateParameters("postgresql");

        // Act
        var result = sampler.ShouldSample(parameters);

        // Assert
        result.Decision.ShouldBe(SamplingDecision.RecordAndSample);
    }

    [Fact]
    public void ShouldSample_WhenParentedSpanWithParentBasedInner_ShouldKeepRecordedParentChild()
    {
        // Arrange
        var sampler = new RootSpanExclusionSampler(
            ["postgresql"],
            new ParentBasedSampler(new AlwaysOnSampler())
        );
        var parameters = CreateParameters("postgresql", CreateParentContext());

        // Act
        var result = sampler.ShouldSample(parameters);

        // Assert
        result.Decision.ShouldBe(SamplingDecision.RecordAndSample);
    }

    [Fact]
    public void ShouldSample_WhenExcludedRootDropped_ShouldAlsoDropItsChildren()
    {
        // Arrange
        var sampler = new RootSpanExclusionSampler(
            ["wolverine_node_assignments"],
            new ParentBasedSampler(new AlwaysOnSampler())
        );
        var parent = CreateParentContext(ActivityTraceFlags.None);
        var parameters = CreateParameters("postgresql", parent);

        // Act
        var result = sampler.ShouldSample(parameters);

        // Assert
        result.Decision.ShouldBe(SamplingDecision.Drop);
    }

    [Fact]
    public void Constructor_WhenExcludedNamesNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        string[]? names = null;

        // Act + Assert
        Should.Throw<ArgumentNullException>(() =>
            new RootSpanExclusionSampler(names!, _innerSampler)
        );
    }

    [Fact]
    public void Constructor_WhenInnerSamplerNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        Sampler? inner = null;

        // Act + Assert
        Should.Throw<ArgumentNullException>(() =>
            new RootSpanExclusionSampler(["postgresql"], inner!)
        );
    }

    [Fact]
    public void Constructor_WhenExcludedNameWhitespace_ShouldThrowArgumentException()
    {
        // Arrange
        var names = new[] { "postgresql", " " };

        // Act + Assert
        Should.Throw<ArgumentException>(() => new RootSpanExclusionSampler(names, _innerSampler));
    }

    private static SamplingParameters CreateParameters(string name, ActivityContext? parent = null)
    {
        return new SamplingParameters(
            parent ?? default,
            ActivityTraceId.CreateRandom(),
            name,
            ActivityKind.Client
        );
    }

    private static ActivityContext CreateParentContext(
        ActivityTraceFlags traceFlags = ActivityTraceFlags.Recorded
    )
    {
        return new ActivityContext(
            ActivityTraceId.CreateRandom(),
            ActivitySpanId.CreateRandom(),
            traceFlags
        );
    }
}

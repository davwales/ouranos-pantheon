namespace Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering.Schemas;

public sealed record CompositeFilterNode(
    LogicalOperator Logic,
    IReadOnlyList<FilterNode> Children
) : FilterNode;

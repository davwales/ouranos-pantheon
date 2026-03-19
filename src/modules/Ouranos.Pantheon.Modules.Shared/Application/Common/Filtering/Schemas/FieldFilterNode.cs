namespace Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering.Schemas;

public sealed record FieldFilterNode(
    string Field,
    FilterOperator Operator,
    string? Value
) : FilterNode;

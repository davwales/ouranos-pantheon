namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Filtering.Schemas;

public sealed record FieldFilterNode(string Field, FilterOperator Operator, string? Value)
    : FilterNode;

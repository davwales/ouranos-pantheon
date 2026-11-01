using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Filtering;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Filtering.Schemas;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Application.Common.Filtering;

public sealed class FilterExpressionTests
{
    [Fact]
    public void For_ShouldWriteFieldOperatorAndValue()
    {
        // Act
        var expression = FilterExpression.For("SymbolId", FilterOperator.Eq, "abc");

        // Assert
        expression.ShouldBe("SymbolId:eq:abc");
    }

    [Theory]
    [InlineData(FilterOperator.Eq)]
    [InlineData(FilterOperator.Neq)]
    [InlineData(FilterOperator.Lt)]
    [InlineData(FilterOperator.Lte)]
    [InlineData(FilterOperator.Gt)]
    [InlineData(FilterOperator.Gte)]
    [InlineData(FilterOperator.Like)]
    [InlineData(FilterOperator.StartsWith)]
    [InlineData(FilterOperator.EndsWith)]
    public void For_ShouldRoundTripThroughFilterParser(FilterOperator filterOperator)
    {
        // Arrange
        var expression = FilterExpression.For("Name", filterOperator, "sword");

        // Act
        var node = FilterParser.Parse(expression);

        // Assert
        node.ShouldBe(new FieldFilterNode("Name", filterOperator, "sword"));
    }

    [Fact]
    public void Operator_ShouldBeLowerCase()
    {
        // Act
        var token = FilterExpression.Operator(FilterOperator.StartsWith);

        // Assert
        token.ShouldBe("startswith");
    }
}

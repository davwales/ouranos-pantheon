using Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering.Schemas;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Application.Common.Filtering;

public sealed class FilterParserTests
{
    [Fact]
    public void Parse_WhenLeafWithValue_ShouldReturnFieldFilterNode()
    {
        // Arrange & Act
        var result = FilterParser.Parse("price:gt:100");

        // Assert
        var node = result.ShouldBeOfType<FieldFilterNode>();
        node.Field.ShouldBe("price");
        node.Operator.ShouldBe(FilterOperator.Gt);
        node.Value.ShouldBe("100");
    }

    [Fact]
    public void Parse_WhenNullOperator_ShouldReturnNodeWithNullValue()
    {
        // Arrange & Act
        var result = FilterParser.Parse("deletedAt:null");

        // Assert
        var node = result.ShouldBeOfType<FieldFilterNode>();
        node.Field.ShouldBe("deletedAt");
        node.Operator.ShouldBe(FilterOperator.Null);
        node.Value.ShouldBeNull();
    }

    [Fact]
    public void Parse_WhenNotNullOperator_ShouldReturnNodeWithNullValue()
    {
        // Arrange & Act
        var result = FilterParser.Parse("deletedAt:notnull");

        // Assert
        var node = result.ShouldBeOfType<FieldFilterNode>();
        node.Operator.ShouldBe(FilterOperator.NotNull);
        node.Value.ShouldBeNull();
    }

    [Theory]
    [InlineData("eq", FilterOperator.Eq)]
    [InlineData("neq", FilterOperator.Neq)]
    [InlineData("lt", FilterOperator.Lt)]
    [InlineData("lte", FilterOperator.Lte)]
    [InlineData("gt", FilterOperator.Gt)]
    [InlineData("gte", FilterOperator.Gte)]
    [InlineData("like", FilterOperator.Like)]
    [InlineData("startswith", FilterOperator.StartsWith)]
    [InlineData("endswith", FilterOperator.EndsWith)]
    [InlineData("in", FilterOperator.In)]
    public void Parse_WhenValueOperator_ShouldParseCorrectOperator(
        string opStr,
        FilterOperator expectedOp
    )
    {
        // Arrange & Act
        var result = FilterParser.Parse($"field:{opStr}:value");

        // Assert
        var node = result.ShouldBeOfType<FieldFilterNode>();
        node.Operator.ShouldBe(expectedOp);
    }

    [Fact]
    public void Parse_WhenOperatorIsMixedCase_ShouldParseSuccessfully()
    {
        // Arrange & Act
        var result = FilterParser.Parse("name:EQ:foo");

        // Assert
        var node = result.ShouldBeOfType<FieldFilterNode>();
        node.Operator.ShouldBe(FilterOperator.Eq);
    }

    [Fact]
    public void Parse_WhenAndGroup_ShouldReturnCompositeNodeWithAndLogic()
    {
        // Arrange & Act
        var result = FilterParser.Parse("and(name:eq:sword|price:gt:100)");

        // Assert
        var node = result.ShouldBeOfType<CompositeFilterNode>();
        node.Logic.ShouldBe(LogicalOperator.And);
        node.Children.Count.ShouldBe(2);
        node.Children[0].ShouldBeOfType<FieldFilterNode>().Field.ShouldBe("name");
        node.Children[1].ShouldBeOfType<FieldFilterNode>().Field.ShouldBe("price");
    }

    [Fact]
    public void Parse_WhenOrGroup_ShouldReturnCompositeNodeWithOrLogic()
    {
        // Arrange & Act
        var result = FilterParser.Parse("or(status:eq:active|status:eq:pending)");

        // Assert
        var node = result.ShouldBeOfType<CompositeFilterNode>();
        node.Logic.ShouldBe(LogicalOperator.Or);
        node.Children.Count.ShouldBe(2);
    }

    [Fact]
    public void Parse_WhenGroupKeywordIsMixedCase_ShouldParseSuccessfully()
    {
        // Arrange & Act
        var result = FilterParser.Parse("AND(a:eq:1|b:eq:2)");

        // Assert
        result.ShouldBeOfType<CompositeFilterNode>().Logic.ShouldBe(LogicalOperator.And);
    }

    [Fact]
    public void Parse_WhenNestedGroup_ShouldReturnCorrectTree()
    {
        // Arrange & Act
        var result = FilterParser.Parse("and(or(a:eq:1|a:eq:2)|b:eq:3)");

        // Assert
        var root = result.ShouldBeOfType<CompositeFilterNode>();
        root.Logic.ShouldBe(LogicalOperator.And);
        root.Children.Count.ShouldBe(2);

        var inner = root.Children[0].ShouldBeOfType<CompositeFilterNode>();
        inner.Logic.ShouldBe(LogicalOperator.Or);
        inner.Children.Count.ShouldBe(2);

        root.Children[1].ShouldBeOfType<FieldFilterNode>().Field.ShouldBe("b");
    }

    [Fact]
    public void Parse_WhenMissingColon_ShouldThrowFormatException()
    {
        // Arrange & Act
        var parse = () => FilterParser.Parse("invalidexpression");

        // Assert
        parse.ShouldThrow<FormatException>();
    }

    [Fact]
    public void Parse_WhenUnknownOperator_ShouldThrowFormatException()
    {
        // Arrange & Act
        var parse = () => FilterParser.Parse("field:unknown:value");

        // Assert
        parse.ShouldThrow<FormatException>();
    }

    [Fact]
    public void Parse_WhenValueRequiredButMissing_ShouldThrowFormatException()
    {
        // Arrange & Act
        var parse = () => FilterParser.Parse("field:eq");

        // Assert
        parse.ShouldThrow<FormatException>();
    }

    [Fact]
    public void Parse_WhenNullOrWhitespace_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var parseNull = () => FilterParser.Parse(null!);
        var parseWhitespace = () => FilterParser.Parse("   ");

        // Assert
        parseNull.ShouldThrow<ArgumentException>();
        parseWhitespace.ShouldThrow<ArgumentException>();
    }
}

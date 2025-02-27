using System.Linq.Expressions;
using HotChocolate.Configuration;
using HotChocolate.Data.Filters;
using HotChocolate.Data.Filters.Expressions;
using HotChocolate.Internal;
using HotChocolate.Language;
using HotChocolate.Types;
using Ouranos.Pantheon.Core.API.FieldHandlers;

namespace Ouranos.Pantheon.Core.API.Tests.FieldHandlers;

public sealed class QueryableStringInvariantHandlerTests
{
    private readonly QueryableStringInvariantHandler _stringHandler;

    public QueryableStringInvariantHandlerTests()
    {
        var inputParser = new InputParser();
        _stringHandler = new QueryableStringInvariantHandler(inputParser);
    }

    [Theory]
    [InlineData(DefaultFilterOperations.Equals)]
    [InlineData(DefaultFilterOperations.StartsWith)]
    [InlineData(DefaultFilterOperations.EndsWith)]
    [InlineData(DefaultFilterOperations.Contains)]
    [InlineData(DefaultFilterOperations.NotEquals)]
    [InlineData(DefaultFilterOperations.NotStartsWith)]
    [InlineData(DefaultFilterOperations.NotEndsWith)]
    [InlineData(DefaultFilterOperations.NotContains)]
    public void CanHandle_WhenSupportedType_ShouldReturnTrue(int operationId)
    {
        // Arrange
        var context = Substitute.For<ITypeCompletionContext>();
        var typeDefinition = Substitute.For<IFilterInputTypeDefinition>();
        var fieldDefinition = new FilterOperationFieldDefinition
        {
            Id = operationId
        };

        context.Type.Returns(Substitute.For<StringOperationFilterInputType>());

        // Act
        var actualResult = _stringHandler.CanHandle(
            context,
            typeDefinition,
            fieldDefinition
        );

        // Assert
        actualResult.ShouldBeTrue();
    }

    [Fact]
    public void CanHandle_WhenUnsupportedType_ShouldReturnFalse()
    {
        // Arrange
        var context = Substitute.For<ITypeCompletionContext>();
        var typeDefinition = Substitute.For<IFilterInputTypeDefinition>();
        var fieldDefinition = new FilterOperationFieldDefinition
        {
            Id = DefaultFilterOperations.Equals
        };

        context.Type.Returns(Substitute.For<BooleanOperationFilterInputType>());

        // Act
        var actualResult = _stringHandler.CanHandle(
            context,
            typeDefinition,
            fieldDefinition
        );

        // Assert
        actualResult.ShouldBeFalse();
    }

    [Fact]
    public void CanHandle_WhenUnsupportedFilterOperation_ShouldReturnFalse()
    {
        // Arrange
        var context = Substitute.For<ITypeCompletionContext>();
        var typeDefinition = Substitute.For<IFilterInputTypeDefinition>();
        var fieldDefinition = new FilterOperationFieldDefinition
        {
            Id = DefaultFilterOperations.GreaterThan
        };

        context.Type.Returns(Substitute.For<StringOperationFilterInputType>());

        // Act
        var actualResult = _stringHandler.CanHandle(
            context,
            typeDefinition,
            fieldDefinition
        );

        // Assert
        actualResult.ShouldBeFalse();
    }

    [Theory]
    [InlineData(DefaultFilterOperations.Equals, nameof(string.Equals))]
    [InlineData(DefaultFilterOperations.StartsWith, nameof(string.StartsWith))]
    [InlineData(DefaultFilterOperations.EndsWith, nameof(string.EndsWith))]
    [InlineData(DefaultFilterOperations.Contains, nameof(string.Contains))]
    public void HandleOperation_WhenInclusiveOperation_ShouldReturnExpectedExpression(
        int operationId,
        string methodName
    )
    {
        // Arrange
        var filterOperation = Substitute.For<IFilterOperationField>();
        var context = SetupContext(filterOperation, operationId);
        var valueNode = Substitute.For<IValueNode>();
        var property = Expression.Parameter(typeof(string), "_s0");
        var toLower = typeof(string).GetMethod(nameof(string.ToLower), [])!;
        const string parsedValue = "some value";

        // Act
        var actualExpression = _stringHandler.HandleOperation(
            context,
            filterOperation,
            valueNode,
            parsedValue
        );

        // Assert
        actualExpression.ShouldBeEquivalentTo(Expression.Call(
            Expression.Call(property, toLower),
            typeof(string).GetMethod(methodName, [typeof(string)])!,
            Expression.Constant(parsedValue.ToLower())
        ));
    }

    [Theory]
    [InlineData(DefaultFilterOperations.NotEquals, nameof(string.Equals))]
    [InlineData(DefaultFilterOperations.NotStartsWith, nameof(string.StartsWith))]
    [InlineData(DefaultFilterOperations.NotEndsWith, nameof(string.EndsWith))]
    [InlineData(DefaultFilterOperations.NotContains, nameof(string.Contains))]
    public void HandleOperation_WhenExclusiveOperation_ShouldReturnExpectedExpression(
        int operationId,
        string methodName
    )
    {
        // Arrange
        var filterOperation = Substitute.For<IFilterOperationField>();
        var context = SetupContext(filterOperation, operationId);
        var valueNode = Substitute.For<IValueNode>();
        var property = Expression.Parameter(typeof(string), "_s0");
        var toLower = typeof(string).GetMethod(nameof(string.ToLower), [])!;
        const string parsedValue = "some value";

        // Act
        var actualExpression = _stringHandler.HandleOperation(
            context,
            filterOperation,
            valueNode,
            parsedValue
        );

        // Assert
        actualExpression.ShouldBeEquivalentTo(Expression.Not(
            Expression.Call(
                Expression.Call(property, toLower),
                typeof(string).GetMethod(methodName, [typeof(string)])!,
                Expression.Constant(parsedValue.ToLower())
            )
        ));
    }

    private static QueryableFilterContext SetupContext(
        IFilterOperationField filterOperation,
        int operationId
    )
    {
        var extendedType = Substitute.For<IExtendedType>();
        var filterInputType = Substitute.For<IFilterInputType>();
        extendedType.Type.Returns(typeof(string));
        extendedType.Source.Returns(typeof(string));
        filterInputType.EntityType.Returns(extendedType);
        filterOperation.Id.Returns(operationId);
        return new QueryableFilterContext(filterInputType, true);
    }
}
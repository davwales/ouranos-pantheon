using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Ouranos.Pantheon.Core.Infra.Mongo.Common;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Core.Infra.Mongo.Tests.Common;

public sealed class QueryExecutorTests
{
    private readonly QueryExecutor _queryExecutor;

    public QueryExecutorTests()
    {
        var logger = Substitute.For<ILogger<QueryExecutor>>();
        _queryExecutor = new QueryExecutor(logger);
    }

    [Fact]
    public async Task FirstOrDefault_WhenValidQueryable_ShouldReturnExpectedResult()
    {
        // Arrange
        var fixture = new Fixture();
        var query = Substitute.For<IMongoQueryable<TestEntity>>();
        var mongoProvider = Substitute.For<IMongoQueryProvider>();
        var cts = new CancellationTokenSource();
        var expectedEntity = fixture.Create<TestEntity>();

        query.Expression.Returns(Expression.Constant(query));
        query.Provider.Returns(mongoProvider);

        mongoProvider
            .ExecuteAsync<TestEntity>(
                Arg.Any<Expression>(),
                cts.Token
            )
            .Returns(expectedEntity);

        // Act
        var actualEntity = await _queryExecutor.FirstOrDefault(query, cts.Token);

        // Assert
        actualEntity.ShouldBe(expectedEntity);
    }

    [Fact]
    public async Task FirstOrDefault_WhenNotMongoQueryable_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var query = Substitute.For<IQueryable<TestEntity>>();

        // Act
        var get = async () => await _queryExecutor.FirstOrDefault(query);

        // Assert
        var actualException = await get.ShouldThrowAsync<InvalidOperationException>();
        actualException.Message.ShouldBe("Cannot perform FirstOrDefaultAsync on a non-Mongo queryable.");
    }

    [Fact]
    public async Task ToList_WhenValidQueryable_ShouldReturnExpectedResult()
    {
        // Arrange
        var fixture = new Fixture();
        var query = Substitute.For<IMongoQueryable<TestEntity>>();
        var cursor = Substitute.For<IAsyncCursor<TestEntity>>();
        var cts = new CancellationTokenSource();
        var expectedResult = fixture.CreateMany<TestEntity>().ToList();

        query.ToCursorAsync(Arg.Any<CancellationToken>()).Returns(cursor);
        cursor.MoveNextAsync(cts.Token).Returns(true, false);
        cursor.Current.Returns(expectedResult);

        // Act
        var actualEntity = await _queryExecutor.ToList(query, cts.Token);

        // Assert
        actualEntity.ShouldBe(expectedResult);
    }

    [Fact]
    public async Task ToList_WhenNotMongoQueryable_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var query = Substitute.For<IQueryable<TestEntity>>();

        // Act
        var get = async () => await _queryExecutor.ToList(query);

        // Assert
        var actualException = await get.ShouldThrowAsync<InvalidOperationException>();
        actualException.Message.ShouldBe("Cannot perform ToListAsync on a non-Mongo queryable.");
    }
}
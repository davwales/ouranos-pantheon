using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Core.Infra.Mongo.Common;
using Ouranos.Pantheon.Tests.Utils;
using Ouranos.Pantheon.Tests.Utils.NSubstitute;

namespace Ouranos.Pantheon.Core.Infra.Mongo.Tests.Common;

public sealed class RepositoryTests
{
    private readonly IMongoCollection<TestEntity> _mongoCollection;
    private readonly IMongoRepository<TestEntity> _mongoRepository;
    private readonly Repository<TestEntity> _repository;

    public RepositoryTests()
    {
        _mongoCollection = Substitute.For<IMongoCollection<TestEntity>>();

        _mongoRepository = Substitute.For<IMongoRepository<TestEntity>>();
        _mongoRepository.GetCollection().Returns(_mongoCollection);

        _repository = new Repository<TestEntity>(
            Substitute.For<ILogger<Repository<TestEntity>>>(),
            _mongoRepository
        );
    }

    [Fact]
    public void CreateId_ShouldReturnObjectId()
    {
        // Act
        var actualId = _repository.CreateId();

        // Assert
        ObjectId.TryParse(actualId.Value, out _).ShouldBeTrue();
    }

    [Fact]
    public async Task Create_ShouldInvokeExpectedActions()
    {
        // Arrange
        var fixture = new Fixture();
        var expectedEntity = fixture.Create<TestEntity>();
        var cts = new CancellationTokenSource();

        // Act
        await _repository.Create(expectedEntity, cts.Token);

        // Assert
        await _mongoCollection.Received(1).InsertOneAsync(expectedEntity, cancellationToken: cts.Token);
    }

    [Fact]
    public async Task CreateMany_ShouldInvokeExpectedActions()
    {
        // Arrange
        var fixture = new Fixture();
        var expectedEntities = fixture.CreateMany<TestEntity>().ToList();
        var cts = new CancellationTokenSource();

        // Act
        await _repository.CreateMany(expectedEntities, cts.Token);

        // Assert
        await _mongoCollection.Received(1).InsertManyAsync(expectedEntities, cancellationToken: cts.Token);
    }

    [Fact]
    public async Task Read_ShouldInvokeExpectedActionsAndReturnExpectedEntity()
    {
        // Arrange
        var fixture = new Fixture();
        var cts = new CancellationTokenSource();
        var cursor = Substitute.For<IAsyncCursor<TestEntity>>();
        var expectedEntity = fixture.Create<TestEntity>();
        var expectedFilter = Builders<TestEntity>.Filter.Eq(x => x.Id, expectedEntity.Id);

        cursor.MoveNextAsync(cts.Token).Returns(true, false);
        cursor.Current.Returns([expectedEntity]);

        _mongoCollection
            .FindAsync(
                ArgExtensions.IsEquivalent(expectedFilter),
                Arg.Any<FindOptions<TestEntity>>(),
                cts.Token
            )
            .Returns(cursor);

        // Act
        var actualEntity = await _repository.Read(expectedEntity.Id, cts.Token);

        // Assert
        actualEntity.ShouldBe(expectedEntity);
    }

    [Fact]
    public async Task ReadAll_ShouldInvokeExpectedActionsAndReturnExpectedEntities()
    {
        // Arrange
        var fixture = new Fixture();
        var cts = new CancellationTokenSource();
        var cursor = Substitute.For<IAsyncCursor<TestEntity>>();
        var expectedEntities = fixture.CreateMany<TestEntity>().ToList();
        var expectedFilter = Builders<TestEntity>.Filter.Empty;

        cursor.MoveNextAsync(cts.Token).Returns(true, false);
        cursor.Current.Returns(expectedEntities);

        _mongoCollection
            .FindAsync(
                ArgExtensions.IsEquivalent(expectedFilter),
                Arg.Any<FindOptions<TestEntity>>(),
                cts.Token
            )
            .Returns(cursor);

        // Act
        var actualEntities = await _repository.ReadAll(cts.Token);

        // Assert
        actualEntities.ShouldBe(expectedEntities);
    }

    [Fact]
    public async Task Update_WhenEntityMatched_ShouldInvokeExpectedActions()
    {
        // Arrange
        var fixture = new Fixture();
        var expectedEntity = fixture.Create<TestEntity>();
        var cts = new CancellationTokenSource();
        var filter = Builders<TestEntity>.Filter.Eq(x => x.Id, expectedEntity.Id);
        var mongoResult = Substitute.For<ReplaceOneResult>();

        mongoResult.MatchedCount.Returns(1);

        _mongoCollection
            .ReplaceOneAsync(
                Arg.Any<FilterDefinition<TestEntity>>(),
                Arg.Any<TestEntity>(),
                ArgExtensions.IsEquivalent(new ReplaceOptions()),
                Arg.Any<CancellationToken>()
            )
            .Returns(mongoResult);

        // Act
        await _repository.Update(expectedEntity, cts.Token);

        // Assert
        await _mongoCollection
            .Received(1)
            .ReplaceOneAsync(
                ArgExtensions.IsEquivalent(filter),
                expectedEntity,
                ArgExtensions.IsEquivalent(new ReplaceOptions()),
                cts.Token
            );
    }

    [Fact]
    public async Task Update_WhenNoEntityMatched_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var fixture = new Fixture();
        var expectedEntity = fixture.Create<TestEntity>();
        var mongoResult = Substitute.For<ReplaceOneResult>();

        mongoResult.MatchedCount.Returns(0);

        _mongoCollection
            .ReplaceOneAsync(
                Arg.Any<FilterDefinition<TestEntity>>(),
                Arg.Any<TestEntity>(),
                Arg.Any<ReplaceOptions>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(mongoResult);

        // Act
        var update = async () => await _repository.Update(expectedEntity);

        // Assert
        var actualException = await update.ShouldThrowAsync<KeyNotFoundException>();
        actualException.Message.ShouldBe($"Could not find TestEntity '{expectedEntity.Id}' to update.");
    }

    [Fact]
    public async Task Delete_WhenEntityDeleted_ShouldInvokeExpectedActions()
    {
        // Arrange
        var fixture = new Fixture();
        var entityId = fixture.Create<Id<TestEntity>>();
        var cts = new CancellationTokenSource();
        var mongoResult = Substitute.For<DeleteResult>();
        var expectedFilter = Builders<TestEntity>.Filter.Eq(x => x.Id, entityId);

        mongoResult.DeletedCount.Returns(1);

        _mongoCollection
            .DeleteOneAsync(
                Arg.Any<FilterDefinition<TestEntity>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(mongoResult);

        // Act
        await _repository.Delete(entityId, cts.Token);

        // Assert
        await _mongoCollection
            .Received(1)
            .DeleteOneAsync(
                ArgExtensions.IsEquivalent(expectedFilter),
                cts.Token
            );
    }

    [Fact]
    public async Task Delete_WhenNoEntityDeleted_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var fixture = new Fixture();
        var entityId = fixture.Create<Id<TestEntity>>();
        var cts = new CancellationTokenSource();
        var mongoResult = Substitute.For<DeleteResult>();

        mongoResult.DeletedCount.Returns(0);

        _mongoCollection
            .DeleteOneAsync(
                Arg.Any<FilterDefinition<TestEntity>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(mongoResult);

        // Act
        var delete = async () => await _repository.Delete(entityId, cts.Token);

        // Assert
        var actualException = await delete.ShouldThrowAsync<KeyNotFoundException>();
        actualException.Message.ShouldBe($"Could not find TestEntity '{entityId}' to delete.");
    }

    [Fact]
    public async Task Delete_WhenGivenPredicate_ShouldReturnExpectedValue()
    {
        // Arrange
        var fixture = new Fixture();
        var predicate = fixture.Create<Expression<Func<TestEntity, bool>>>();
        var cts = new CancellationTokenSource();
        var mongoResult = Substitute.For<DeleteResult>();
        var expectedFilter = Builders<TestEntity>.Filter.Where(predicate);
        var expectedResult = fixture.Create<long>();

        mongoResult.DeletedCount.Returns(expectedResult);

        _mongoCollection
            .DeleteManyAsync(
                Arg.Any<FilterDefinition<TestEntity>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(mongoResult);

        // Act
        var actualResult = await _repository.Delete(predicate, cts.Token);

        // Assert
        actualResult.ShouldBe(expectedResult);

        await _mongoCollection
            .Received(1)
            .DeleteManyAsync(
                ArgExtensions.IsEquivalent(expectedFilter),
                cts.Token
            );
    }

    [Fact]
    public async Task Upsert_ShouldInvokeExpectedActions()
    {
        // Arrange
        var fixture = new Fixture();
        var expectedEntity = fixture.Create<TestEntity>();
        var cts = new CancellationTokenSource();
        var filter = Builders<TestEntity>.Filter.Eq(x => x.Id, expectedEntity.Id);
        var replaceOptions = new ReplaceOptions { IsUpsert = true };

        // Act
        await _repository.Upsert(expectedEntity, cts.Token);

        // Assert
        await _mongoCollection
            .Received(1)
            .ReplaceOneAsync(
                ArgExtensions.IsEquivalent(filter),
                expectedEntity,
                ArgExtensions.IsEquivalent(replaceOptions),
                cts.Token
            );
    }

    [Fact]
    public void AsQueryable_ShouldReturnExpectedQuery()
    {
        // Arrange
        var fixture = new Fixture();
        var entities = fixture.CreateMany<TestEntity>().AsQueryable();
        var expectedQuery = entities as IMongoQueryable<TestEntity>;

        _mongoRepository.AsQueryable().Returns(expectedQuery);

        // Act
        var actualQuery = _mongoRepository.AsQueryable();

        // Assert
        actualQuery.ShouldBe(expectedQuery);
    }

    [Fact]
    public async Task FirstOrDefault_ShouldReturnExpectedValue()
    {
        // Arrange
        var fixture = new Fixture();
        var predicate = fixture.Create<Expression<Func<TestEntity, bool>>>();
        var cts = new CancellationTokenSource();
        var cursor = Substitute.For<IAsyncCursor<TestEntity>>();
        var expectedEntity = fixture.Create<TestEntity>();
        var expectedFilter = Builders<TestEntity>.Filter.Where(predicate);

        cursor.MoveNextAsync(cts.Token).Returns(true, false);
        cursor.Current.Returns([expectedEntity]);

        _mongoCollection
            .FindAsync(
                ArgExtensions.IsEquivalent(expectedFilter),
                Arg.Any<FindOptions<TestEntity>>(),
                cts.Token
            )
            .Returns(cursor);

        // Act
        var actualEntity = await _repository.FirstOrDefault(predicate, cts.Token);

        // Assert
        actualEntity.ShouldBe(expectedEntity);
    }

    [Fact]
    public async Task Any_ShouldReturnExpectedValue()
    {
        // Arrange
        var fixture = new Fixture();
        var predicate = fixture.Create<Expression<Func<TestEntity, bool>>>();
        var cts = new CancellationTokenSource();
        var cursor = Substitute.For<IAsyncCursor<BsonDocument>>();
        var document = fixture.Create<BsonDocument>();
        var expectedFilter = Builders<TestEntity>.Filter.Where(predicate);

        cursor.MoveNextAsync(cts.Token).Returns(true, false);
        cursor.Current.Returns([document]);

        _mongoCollection
            .FindAsync(
                ArgExtensions.IsEquivalent(expectedFilter),
                Arg.Any<FindOptions<TestEntity, BsonDocument>>(),
                cts.Token
            )
            .Returns(cursor);

        // Act
        var actualResult = await _repository.Any(predicate, cts.Token);

        // Assert
        actualResult.ShouldBeTrue();
    }
}
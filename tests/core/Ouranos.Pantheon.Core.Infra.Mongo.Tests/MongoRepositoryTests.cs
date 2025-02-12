using MongoDB.Driver;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Core.Infra.Mongo.Tests;

public sealed class MongoRepositoryTests
{
    private readonly IMongoDatabase _mongoDatabase;
    private readonly MongoRepository<TestEntity> _repository;

    public MongoRepositoryTests()
    {
        _mongoDatabase = Substitute.For<IMongoDatabase>();

        var mongoDatabaseManager = Substitute.For<IMongoDatabaseManager>();
        mongoDatabaseManager.GetDatabase<TestEntity>().Returns(_mongoDatabase);

        _repository = new MongoRepository<TestEntity>(mongoDatabaseManager);
    }

    [Fact]
    public void GetCollection_ShouldRetrieveExpectedCollection()
    {
        // Arrange
        var expectedCollection = Substitute.For<IMongoCollection<TestEntity>>();
        _mongoDatabase.GetCollection<TestEntity>("testentitys").Returns(expectedCollection);

        // Act
        var actualCollection = _repository.GetCollection();

        // Assert
        actualCollection.ShouldBe(expectedCollection);
    }

    [Fact]
    public void AsQueryable_ShouldRequestMongoQueryable()
    {
        // Arrange
        var collection = Substitute.For<IMongoCollection<TestEntity>>();

        _mongoDatabase.GetCollection<TestEntity>(Arg.Any<string>()).Returns(collection);

        // Act
        var get = () => _repository.AsQueryable();

        // Assert - This logic happens in Mongo SDK extension methods, so we just verify it made an attempt.
        get.ShouldThrow<NullReferenceException>();
        collection.Database.Client.Settings.ShouldBeNull();
        collection.Database.Client.Received(1);
    }
}